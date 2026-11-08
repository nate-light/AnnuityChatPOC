export function scrollToBottom(id) {
    const element = document.getElementById(id);
    if (!element) {
        return;
    }

    element.scrollTop = element.scrollHeight;
}

