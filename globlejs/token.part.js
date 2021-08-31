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