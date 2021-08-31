// ================================================
// 项目全局js库,包含接口约定.具体功能根据项目具体实现.
// ================================================
((win) => {
    // ----------------------
    // window对象上的命名空间.
    // ----------------------
    let ns = {};

    // --------------------------------------------
    // 项目命名空间:页面脚本封闭函数中需要提供到外部使用的,
    // 对象,变量,函数,页面间传值等,统一绑定在此对象
    // --------------------------------------------
    ns.page = {};

// --------------------------------------------
// cfg:公用配置对象. 方法小写开头,属性大写开头
// --------------------------------------------
let cfg = {};

// API 服务器地址,返回一个全路径地址.
cfg.apiUrl = (path) => { return 'http://127.0.0.1:8080' + path; };

ns.cfg = cfg;
// --------------------------------------------
// 预定错误提示语
// --------------------------------------------
let errtxt = {
    // 3位数约定为固定错误
    200: '服务器返回成功',
    500: '服务器发生异常',
    510: '拒绝请求',
    600: '操作失败',
    601: '没有数据',
    602: '参数错误',
    603: '数据库错误',
    // 4位数约定为自定义错误
    4001: '更新失败,源数据错误',
    4002: '未更新,内容没有修改',
    4003: '更新失败,无权操作'
};
// --
ns.errtxt = errtxt;
// -----------------
// 客户端token
// -----------------
let token = {};
let tokenkey = 'token';
// 存
token.newToken = (token) => {
    let tk = { token: token };
    localStorage.setItem(tokenkey, JSON.stringify(tk));
};
// 取
token.get = () => {
    let tkjson = localStorage.getItem(tokenkey);
    if (!tkjson) return null;
    let tk = JSON.parse(tkjson);
    return tk.token;
};
// 删除
token.del = () => {
    localStorage.removeItem(tokenkey);
    // 清除登录信息缓存
    sessionStorage.removeItem('loginid');
};

ns.token = token;
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
// --------------------------------------------
// 注册app相关事件的统一位置
// --------------------------------------------
let appevent = {};
ns.appevent = appevent;

// --------------------------------------------
// 主菜单路由
// --------------------------------------------
let router = {};
// 当前活动菜单路径
router.url = cfg.apiUrl('/');
/**
 * 路由跳转url.一个函数,自定义实现
 * @param {string} url 菜单url
 * @param {any} para 页面传递参数.默认null
 */
router.goto = (url, para = null) => { };
// 路由参数 {any}
router.para = null;

// 所有url,值是一个对象,包含url,title属性.也可以加其它属性 {urlkey:{url:,title:,}}
router.urls = {}
ns.router = router;
// 引用名称
win.ns = ns;
}) (window);