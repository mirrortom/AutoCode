// --------------------------------------------
// 项目相关工具函数
// --------------------------------------------
let tool = {};

// 公用带token的ajax
tool.get = (url, para, resType = 'html') => {
    let initCfg = { headers: { 'Auth': ns.token.get() } };
    return $.get(url, para, initCfg, resType);
};
tool.post = (url, para, resType = 'json') => {
    let initCfg = { headers: { 'Auth': ns.token.get() } };
    return $.post(url, para, initCfg, resType);
};
tool.getAsync = async (url, para, resType = 'html') => {
    let initCfg = { headers: { 'Auth': ns.token.get() } };
    return await $.getAsync(url, para, initCfg, resType);
};
tool.postAsync = async (url, para, resType = 'json') => {
    let initCfg = { headers: { 'Auth': ns.token.get() } };
    return await $.postAsync(url, para, initCfg, resType);
};

ns.tool = tool;