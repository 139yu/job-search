(() => {
  const url = 'https://www.zhipin.com/wapi/zpgeek/search/joblist.json?scene=1&query=C%23%20%E4%B8%8A%E4%BD%8D%E6%9C%BA&city=101281600&page=1&pageSize=30';
  return fetch(url, {headers: {'accept': 'application/json'}}).then(r => r.text()).then(t => t.slice(0, 20000)).catch(e => 'ERR:' + e.message);
})()
