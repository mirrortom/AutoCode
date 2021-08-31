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