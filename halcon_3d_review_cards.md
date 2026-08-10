# Halcon 3D 复习卡片：平面拟合、距离、平面度、体积、夹角、直线交线

> 用途：面试前背诵用。每张卡片按「30 秒版 -> 2 分钟版 -> 关键算子 -> 面试追问 -> 易错点」组织。
> 案例目录：`D:\Code\Github\halcon_3d\hdev\05点云案例处理`

## 0. 通用框架（先背这个，所有案例都能套）

深度图 -> 按 Scale 换算成点云 -> 去飞点/滤波 -> 画 ROI 提取区域 -> 点云拟合或几何计算 -> 坐标系归一化（主惯量轴/位姿）-> 按 ZScale 换算出毫米结果。

面试开头统一这样说：3D 案例的核心不是单个算子，而是「先把深度图变成带物理单位的点云，再在 ROI 上做几何拟合，最后把结果换回毫米」。这句话能显示你理解全流程，而不是背算子。

## 1. 平面拟合

### 30 秒版

读入深度图后，先画几个矩形 ROI，把 ROI 里的高度值按比例裁掉最高最低 10%，再用鲁棒拟合算出平面；同时把深度图转成点云，通过刚性变换平移到原点方便观察和后续测量。

### 2 分钟版

1. `read_image` 读深度图 `test.tif`，XY 方向 ScaleFactor 0.06、Z 方向 0.001，Z 是毫米级。
2. 交互式画 3 个矩形 ROI，`union1` 合并后 `reduce_domain` 裁剪出测量区域。
3. 取 ROI 内所有点的灰度值（即高度），排序后按 `rateLowRemove / rateHighRemove` 各去掉 10%，防止飞点把拟合带偏。
4. 用 `fit_surface_first_order` 拟合平面（tukey 鲁棒权重），再用 `gen_image_surface_first_order` 生成理想平面图，可直接和原始深度图对比。
5. 用 `depth_map_piontcloud` 把深度图转成点云，拟合平面模型；`rigid_trans_object_model_3d` 平移到原点。
6. `visualize_object_model_3d` 显示，用 `'intensity_1' = 'coord_z'` 按高度着色、alpha 调透明。

### 关键算子

| 算子 | 作用 |
| --- | --- |
| reduce_domain | 按 ROI 裁剪图像域 |
| filter_region_grayval | 高度值排序后裁剪高低百分比，去飞点 |
| fit_surface_first_order | 对灰度图拟合一阶平面 |
| fit_primitives_object_model_3d | 对点云拟合 plane/sphere/cylinder |
| depth_map_piontcloud | 深度图转 3D 点云 |
| rigid_trans_object_model_3d | 平移/旋转点云 |

### 面试追问

为什么裁剪最高最低 10%？点云里常见飞点，高度极值可能来自噪声或边缘，裁剪后拟合更稳。

为什么用 tukey/huber 而不是普通最小二乘？最小二乘把残差平方放大，几个飞点就能拉偏平面；Huber 对大残差降权为线性，Tukey 更狠，直接把离群点权重降到接近 0。

## 2. 点到平面的距离

### 30 秒版

基准面和测量面各画一个 ROI，分别拟合成平面；把测量面的点到基准面的法向距离算出来，再按 Z 方向 Scale 换成毫米。

### 2 分钟版

1. 绿色框是基准面，黄色框是测量面；两框内都按 10% 比例做灰度滤波。
2. 两个 ROI 各自通过 `gen_3d_model_from_points` 转点云并拟合平面，拟合算法是 `least_squares_huber`。
3. 用 `moments_object_model_3d(..., 'principal_axes')` 求主惯量轴得到位姿，`pose_invert` + `pose_to_hom_mat3d` + `affine_trans_object_model_3d` 把点云归一化到局部坐标系，让 Z 轴垂直于基准面。
4. 从拟合平面取点求平面方程系数 A/B/C/D（代码里是向量叉积实现）。
5. 点到平面距离公式：`d = |A*x + B*y + C*z + D| / sqrt(A*A + B*B + C*C)`。
6. 高度 = `Dis / zScale * 0.001`，单位 mm。

### 关键算子

| 算子 | 作用 |
| --- | --- |
| fit_primitives_object_model_3d | 拟合平面 |
| moments_object_model_3d | 求主惯量轴，得到坐标系 |
| pose_invert / pose_to_hom_mat3d / affine_trans_object_model_3d | 位姿转齐次矩阵并施加变换 |
| get_plane_func | 由平面模型取 3 点叉积求 A/B/C/D |

### 面试追问

为什么要归一化到局部坐标系？深度图可能有倾斜或传感器角度差异，直接把像素距离当真实高度会偏；归一化后 Z 轴垂直基准面，距离计算更直观、更稳。

平面方程和法向量是什么关系？`Ax+By+Cz+D=0` 中 `(A,B,C)` 就是法向量，除以它的模就是单位法向量。

### 易错点

`get_plane_func` 是教学式实现：随机在平面上取 3 点叉积求法向量。工程上更稳的是直接读 `primitive_parameter` 或平面位姿，避免随机性。

## 3. 平面度

### 30 秒版

对测量区域拟合成一个理想平面，算出每个点到这个平面的距离，最大值减最小值就是平面度。

### 2 分钟版

1. `gen_3d_model_from_points` 把 ROI 点云转成 3D 模型并拟合平面。
2. 用主惯量轴归一化点云，保证后续计算在局部坐标系。
3. 取 ROI 内所有点的 X/Y/Z，用 `get_plane_func` 得到平面系数 A/B/C/D。
4. 逐点代入点到平面距离公式，得到距离数组。
5. `Theta = max(距离) - min(距离)`，平面度 = `Theta / ZScale * 0.001`，单位 mm。

### 关键算子

| 算子 | 作用 |
| --- | --- |
| gen_3d_model_from_points | 点云 + 平面拟合一体 |
| cal_3d_object_flatness | 案例封装的平面度计算 |
| get_plane_func | 求平面方程系数 |

### 面试追问

平面度为什么是 max-min 而不是 RMS？平面度国标定义就是被测表面相对理想平面的最大变动范围，max-min 描述"最高点-最低点"；RMS 描述整体波动，更像粗糙度或拟合残差评价。

平面度和高度差的区别？高度差是相对某个固定基准（如平台面）的高度变化；平面度是相对自己拟合出的理想平面的最大偏离，和摆放姿态无关。

### 易错点

平面度结果必须换回毫米；直接用像素灰度差讲会露馅，面试官一定会追问单位换算。

## 4. 体积

### 30 秒版

深度图转点云后先采样减点，再贪心三角化成网格；在 ROI 内以底部为参考平面，用 Halcon 的体积算子求点云和底面围成的体积，最后换算单位。

### 2 分钟版

1. `depth_image_to_pointcloud` 把深度图转点云，XYZ 已统一到 mm。
2. `sample_object_model_3d('fast', 3)` 采样，减少点数、加快后续三角化。
3. `triangulate_object_model_3d('greedy')` 贪心三角化，生成带面片的网格模型。
4. 交互式画 ROI，调用自定义函数 `gen_3d_volumn`。
5. 函数内部用 `object_model_3d_to_xyz` 把三角化模型投影成 X/Y/Z 图，`reduce_domain` 按 ROI 裁剪，再 `xyz_to_object_model_3d` 还原成 ROI 点云。
6. `pose_invert` + `rigid_trans_object_model_3d` 做模型对齐；用 ROI 包围盒底部高度生成参考平面 `PosePlane`。
7. 对 ROI 点云再次三角化，用 `volume_object_model_3d_relative_to_plane(..., 'signed', 'true')` 求相对参考平面的有符号体积，取绝对值。

### 关键算子

| 算子 | 作用 |
| --- | --- |
| depth_image_to_pointcloud | 深度图转点云 |
| sample_object_model_3d | 点云采样减点 |
| triangulate_object_model_3d | 贪心三角化生成网格 |
| object_model_3d_to_xyz / xyz_to_object_model_3d | 3D 模型与 XYZ 图互转 |
| volume_object_model_3d_relative_to_plane | 相对参考平面求体积 |

### 面试追问

为什么用相对参考平面而不是直接算绝对体积？体积是表面和底面围成的空间，必须有一个底面基准；相对平面计算可以排除工件摆放高度的影响。

为什么先采样？点数越多三角化越慢，而过度密集的点对体积精度提升有限；采样 3 级是精度和速度的折中。

有符号体积是什么？点云在参考平面上方为正、下方为负；取绝对值得到通用体积。

### 易错点

单位：代码注释里 XYZ 已统一到 mm，体积结果是 mm³；代码里再按 Scale 换算成工程单位，讲的时候要把「毫米和像素的比例」说清楚，这是最容易翻车的地方。

## 5. 平面夹角

### 30 秒版

两个 ROI 各自拟合一个平面，求出两个平面的法向量，用点积算余弦得到夹角；为了避免法向量方向正负造成补角，先把法向量统一翻转到和 Z 轴成锐角的方向。

### 2 分钟版

1. 两个 ROI（绿框、蓝框）分别做 5% 高度滤波，`gen_3d_model_from_points` 各自拟合成平面，算法 `least_squares_huber`。
2. 调 `cal_plane_angle`：先把两个测量面按各自最低高度平移到零点，再用基准面的主惯量轴位姿把点云归一化到同一局部坐标系。
3. `get_plane_func` 得到两个平面的法向量 `n_A`、`n_B`，分别归一化为单位向量。
4. 判断法向量与 Z 轴夹角余弦：若 `cosTheta < 0` 说明是钝角，翻转法向量取反，保证两个法向量都朝上。
5. 夹角余弦 `cosTheta3 = n_A . n_B / (|n_A| * |n_B|)`，`radAB = acos(cosTheta3)`，`PlaneAngle = deg(radAB)` 输出角度。

### 关键算子

| 算子 | 作用 |
| --- | --- |
| gen_3d_model_from_points | 点云转模型并拟合平面 |
| moments_object_model_3d | 基准面主惯量轴，统一坐标系 |
| get_plane_func | 求两平面法向量 |
| acos / deg | 反余弦 + 弧度转角度 |

### 面试追问

为什么要翻转法向量？平面法向量有正负两个方向，直接点积可能算出补角；统一翻转到与 Z 轴成锐角后，点积结果才对应两个平面实际夹角。

夹角为什么不用 Halcon 现成算子？Halcon 有 `angle_ll` 之类 2D 夹角算子，但 3D 平面夹角本质就是法向量夹角，用点积公式最直接，也方便控制方向语义。

### 易错点

角度是平面夹角（0-90 度），不是二面角正负；面试时说清楚「先处理法向量方向，再求夹角」就能体现工程细节。

## 6. 空间直线拟合 + 平面交线提取

已有完整讲解笔记：[halcon_3d_interview_notes.md](halcon_3d_interview_notes.md)

只背三条主线：

1. Halcon 不能直接拟合 3D 直线，工程上两个做法：两个特征区域质心连线（两点定直线），或两个平面求交线。
2. 交线提取流程：点云去噪 -> 三角化 -> ROI -> 30 个平行平面切片 -> `intersect_plane_object_model_3d` 求每条交线 -> 投影成 2D 轮廓 -> 一阶/二阶导找边缘点 -> 反变换回 3D。
3. 为什么投影 2D 再找边缘：可以复用 2D 视觉的边缘检测和角度工具，计算直观，最后反变换回 3D 坐标即可。

## 7. 面试串讲模板（30 秒项目版）

我在 X-Ray 无损检测设备上用 Halcon 做 3D 视觉方案验证，遇到过平面度、高度差、体积、平面夹角这类测量需求。我的统一做法是：深度图先按标定比例转成毫米点云，裁剪 ROI 并做高度滤波去掉飞点，再拟合成平面做几何计算，最后统一换算出工程单位。其中最容易踩坑的是单位换算和飞点，我在案例里验证过采样率、滤波比例和拟合算法的效果。

这样讲既覆盖全流程，又给了面试官追问抓手（滤波、拟合、单位换算）。

## 8. 复习节奏

1. 每晚 30 分钟：背一张卡的 30 秒版 + 2 分钟版，对着镜子或录音讲一遍。
2. 周末 2 小时：把 6 张卡完整过一遍，先默写流程图，再开 `hdev` 把对应案例跑一遍。
3. 重点练 3 个追问：为什么去飞点、为什么用鲁棒拟合、为什么统一坐标系。
4. 每张卡至少完整讲 3 遍，录音回听，卡壳处单独再背。

