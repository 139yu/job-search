# Halcon 3D 面试讲解笔记：空间直线拟合 + 平面交线提取

> 来源：`D:\Code\Github\halcon_3d\hdev\05点云案例处理\08-点云直线拟合\点云直线拟合.hdev`
> Halcon 版本 24.11.1.0。本文按"面试能直接讲出来"整理，先记主干，再补细节。

## 一、30 秒电梯版

我最近系统学完 Halcon 3D 点云处理，完整跑通了两个案例：空间直线拟合和平面交线提取。

整体流程是：先把深度图转成点云，用邻域统计滤波去掉孤立噪点，再三角化生成网格；在 ROI 内取两个特征区域求质心，用两点确定一条空间直线；然后用拟合出的平面去切割三角网格得到交线，把 3D 交线投影成 2D 轮廓，用一阶、二阶导数定位变化最剧烈的边缘点，最后反变换回 3D 坐标系做角度和位置测量。

## 二、2 分钟完整版

### 案例一：空间直线拟合

这个案例的核心思路是"两点定直线"，工程实现上分六步：

1. 深度图转点云：用 `depth_image_to_pointcloud`，通过 Scale 把深度值换算成真实坐标，得到带 XYZ 的点云模型。
2. 点云去噪：用 `get_object_model_3d_params` 算每个点的邻域距离分布，取 70% 分位作为阈值，再用 `select_points_object_model_3d` 做邻域统计滤波，把孤立噪点去掉。这一步很关键，不去噪后面的拟合会被离群点带偏。
3. 三角化：用 `triangulate_object_model_3d` 生成网格，后面交线提取必须在网格上做。
4. 绘制 ROI：在三维视图里用交互式矩形框选目标区域，通过 `object_model_3d_to_xyz` 把 ROI 内点云转成 X/Y/Z 图，再 `xyz_to_object_model_3d` 还原成 ROI 点云模型。
5. 取两个标记点：在 ROI 内自动生成两个小圆区域，用 `get_object_models_center` 求各自质心，得到 Center1、Center2。
6. 两点定直线：用 `create_pose` 构造起点、终点位姿，`gen_arrow_object_model_3d` 生成一条带方向的 3D 直线，方向根据两点 Z 的高低确定。

另外，案例里还用 `fit_primitives_object_model_3d` 对 ROI 点云拟合了平面，取 `primitive_pose` 作为后续切割平面的基准。Halcon 的 `fit_primitives_object_model_3d` 只支持平面、球、圆柱这类几何体，不支持直接拟合 3D 直线，所以工程上空间直线常用两种方式表达：两个特征区域的质心连线，或者两个平面求交线。

### 案例二：平面交线提取

这个案例是用一族平行平面去"切片"点云，每片求交线、再在交线上定位边缘点，本质是把工件边缘线提取出来：

1. 先对 ROI 点云三角化，得到带面片的网格模型。
2. 用第一步拟合出的平面位姿 `PlanePose` 生成一个切割平面，采样后求主惯性轴 `moments_object_model_3d`，把平面变换到局部坐标系。
3. 用 `rigid_trans_object_model_3d` 沿法线方向平移平面，按 `CutNum = 30` 生成 30 个切片，步长取 ROI 盒子的短边除以切片数。
4. 每个切片用 `pose_compose` 把局部位姿和移动量组合回原始坐标系，再 `intersect_plane_object_model_3d` 让平面与三角网格求交，输出 3D 交线模型。
5. 用 `project_object_model_3d` 把 3D 交线投影到临时相机平面，变成 2D 轮廓，方便后续计算和显示。
6. 自定义过程 `get_edge_point` 找边缘点：把轮廓点按列排序，计算每个点到参考箭头线的距离，构造成一维函数，用 `derivate_funct_1d` 求一阶、二阶导，二阶导最大的点就是轮廓变化最剧烈的边缘点。
7. 用 `angle_lx` 计算边缘点连线的角度，再用 `hom_mat2d_rotate` 把轮廓旋转到水平对齐，方便量测。
8. 最后用 `hom_mat2d_invert` 逆变换回 2D 原坐标，再 `pose_to_hom_mat3d` + `affine_trans_point_3d` 反变换回 3D，收集每一片的边缘点坐标，形成工件的一条 3D 边缘线。

## 三、关键算子速查表

| 算子 | 作用 |
| --- | --- |
| depth_image_to_pointcloud | 深度图转点云，Scale 控制单位换算 |
| get_object_model_3d_params | 查点云属性，如邻域距离、点数、包围盒 |
| select_points_object_model_3d | 按属性筛选点，用于去噪 |
| triangulate_object_model_3d | 点云三角化生成网格，交线提取的前置步骤 |
| object_model_3d_to_xyz / xyz_to_object_model_3d | 3D 模型与 XYZ 图互相转换 |
| get_object_models_center | 求 3D 模型质心 |
| fit_primitives_object_model_3d | 拟合几何体，支持 plane/sphere/cylinder |
| gen_arrow_object_model_3d | 用两点位姿生成 3D 箭头，用于表达方向直线 |
| gen_plane_object_model_3d | 按位姿生成平面模型 |
| moments_object_model_3d | 求主惯性轴，用于坐标系归一化 |
| rigid_trans_object_model_3d | 平移/旋转 3D 模型 |
| pose_compose | 组合两个位姿 |
| intersect_plane_object_model_3d | 平面切割 3D 模型，输出交线 |
| project_object_model_3d | 3D 模型投影到相机平面，得到 2D 轮廓 |
| derivate_funct_1d | 一维函数求导，用于找变化剧烈点 |
| angle_lx | 求两点连线与水平轴的夹角 |
| pose_invert / pose_to_hom_mat3d / affine_trans_object_model_3d / affine_trans_point_3d | 位姿与齐次矩阵互相转换，完成坐标系变换 |

## 四、面试追问准备

### 1. 为什么平面拟合用 least_squares_huber？

普通最小二乘会把离群点按平方放大，点云里只要有几个孤立噪点，拟合平面就会偏。Huber 损失对残差小的点用平方权重、残差大的点用线性权重，相当于把离群点的带偏效应压住；Tukey 更狠，直接剔除。设备点云里常见飞点，所以选鲁棒拟合更稳。

### 2. Halcon 没有 3D 直线拟合算子，直线怎么来的？

`fit_primitives_object_model_3d` 只支持平面、球、圆柱。空间直线在工程上两种做法：一是取两个特征区域的质心连线，二是用两个平面求交线。这个案例用的是第一种：ROI 里两个标记圆区域的质心确定起点和终点。

### 3. 为什么交线提取前必须先三角化？

`intersect_plane_object_model_3d` 是在三角网格模型上做切割，只有点没有面片时算不出交线。所以流程是：点云 -> 三角化 -> 平面切割 -> 交线。

### 4. 为什么要把 3D 交线投影成 2D 再找边缘？

两点原因：一是显示和交互方便，边缘检测、角度计算在 2D 轮廓上可以直接复用 2D 视觉那套工具；二是把问题降维后计算更直观。但最后结果要反变换回 3D 坐标系，所以流程里反复在做正变换和逆变换。

### 5. 边缘点为什么用二阶导数找？

边缘的本质是轮廓变化最剧烈的位置，一阶导数的极值对应变化最快，二阶导数的绝对值最大点更精确。案例里先把轮廓点按列排序，每个点到参考箭头的距离构成一维函数，再求导找峰值，等价于在轮廓上找突变点。

### 6. 精度受哪些因素影响？

点云本身质量（传感器噪声、飞点）、去噪阈值、ROI 范围、拟合算法、切片步长、相机标定和 Scale 单位换算都影响精度。切片数越多，边缘点越密，但计算量越大，需要平衡。

### 7. 3D 视觉和 2D 视觉的区别？

2D 只能在图像平面测尺寸、位置，受光照和表面纹理影响大；3D 增加了深度维度，能测高度、平面度、体积、空间角度，对表面纹理不敏感，但依赖传感器精度、标定准确度和更大的计算量。像平面度、高度差、空间角度这类检测，只能靠 3D。

## 五、容易说错的地方

- 不要说"Halcon 直接用算子拟合 3D 直线"，实际是两点定直线或平面求交。
- 不要跳过去噪直接讲拟合，面试官会追问离群点影响。
- 不要混淆 `pose` 和 `hom_mat3d`：位姿适合描述物体位置，齐次矩阵适合做连续变换，代码里两者经常互转。
- 交线提取的对象是三角网格，不是原始点云。

## 六、练习动作

1. 把 `点云直线拟合.hdev` 完整跑通，改三个参数看结果：`NumNeighbor`（25）、`InlierRate`（70）、`CutNum`（30）。
2. 关掉 Debug，用文字把流程默写一遍，再对照上面的速查表补漏。
3. 录音讲 30 秒版，回听检查有没有"嗯、啊"，讲到不卡壳为止。
4. 试着自己回答"为什么用两点定直线""为什么先三角化"这两个追问。
