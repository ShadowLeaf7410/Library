// wwwroot/js/site.js
export function showPopupAndRedirect(seconds, redirectUrl) {
    // 创建弹窗容器
    const popup = document.createElement('div');
    popup.id = 'redirectPopup';

    // 使用CSS对象设置样式（避免字符串拼接）
    Object.assign(popup.style, {
        position: 'fixed',
        top: '50%',
        left: '50%',
        transform: 'translate(-50%, -50%)',
        padding: '20px',
        backgroundColor: 'white',
        border: '1px solid #ccc',
        borderRadius: '5px',
        boxShadow: '0 2px 10px rgba(0,0,0,0.2)',
        zIndex: '10000',
        textAlign: 'center',
        minWidth: '300px'
    });

    // 创建标题元素
    const title = document.createElement('h3');
    title.style.marginTop = '0';
    title.textContent = '操作成功！'; // 直接设置文本内容

    // 创建倒计时文本
    const countdownText = document.createElement('p');
    const countdownSpan = document.createElement('span');
    countdownSpan.id = 'countdown';
    countdownSpan.style.fontWeight = 'bold';
    countdownSpan.style.color = 'red';
    countdownSpan.textContent = seconds.toString();

    countdownText.appendChild(document.createTextNode('将在 '));
    countdownText.appendChild(countdownSpan);
    countdownText.appendChild(document.createTextNode(' 秒后自动跳转...'));

    // 创建立即跳转按钮
    const button = document.createElement('button');
    button.id = 'redirectNow';
    button.textContent = '立即跳转';
    Object.assign(button.style, {
        padding: '8px 16px',
        backgroundColor: '#007bff',
        color: 'white',
        border: 'none',
        borderRadius: '4px',
        cursor: 'pointer',
        marginTop: '10px'
    });

    // 组装弹窗
    popup.appendChild(title);
    popup.appendChild(countdownText);
    popup.appendChild(button);

    // 添加到文档
    document.body.appendChild(popup);

    // 倒计时逻辑
    let remaining = seconds;
    const intervalId = setInterval(() => {
        remaining--;
        countdownSpan.textContent = remaining.toString();

        if (remaining <= 0) {
            clearInterval(intervalId);
            window.location.href = redirectUrl;
        }
    }, 1000);

    // 按钮点击事件
    button.addEventListener('click', () => {
        clearInterval(intervalId);
        window.location.href = redirectUrl;
    });

    return intervalId;
}
function cancelRedirect(intervalId) {
    clearInterval(intervalId);
    const popup = document.getElementById('redirectPopup');
    if (popup) {
        popup.remove();
    }
}