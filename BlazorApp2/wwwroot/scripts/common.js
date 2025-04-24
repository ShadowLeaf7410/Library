console.log("common.js 加载");
window.goBack = function () {
    console.log("调用 goBack");
    window.history.back();
};