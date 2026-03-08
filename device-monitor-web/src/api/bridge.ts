/**
 * WebView2 双向消息桥接工具
 *
 * 前端 → 后端: window.chrome.webview.postMessage(JSON)
 * 后端 → 前端: window.chrome.webview 'message' 事件
 *
 * 消息协议:
 *   请求: { action: string, reqId: string, payload: any }
 *   响应: { reqId: string, ok: boolean, data: any, error?: string }
 */

type PendingCallback = {
    resolve: (data: unknown) => void
    reject: (reason: Error) => void
}

const pending = new Map<string, PendingCallback>()

// 监听后端推送的消息
function initListener() {
    const webview = (window as any).chrome?.webview
    if (!webview) return // 非 WebView2 环境（普通浏览器开发时静默忽略）
    webview.addEventListener('message', (event: MessageEvent) => {
        try {
            const msg = typeof event.data === 'string' ? JSON.parse(event.data) : event.data
            const { reqId, ok, data, error } = msg
            if (!reqId) return // 主动推送消息，暂不处理
            const cb = pending.get(reqId)
            if (!cb) return
            pending.delete(reqId)
            if (ok) {
                cb.resolve(data)
            } else {
                cb.reject(new Error(error ?? '后端返回错误'))
            }
        } catch (e) {
            console.error('[bridge] 消息解析失败', e)
        }
    })
}

initListener()

let _reqCounter = 0
function nextReqId(): string {
    return `req_${++_reqCounter}_${Date.now()}`
}

/**
 * 向后端发送消息并等待响应
 * @param action  后端 action 字符串，如 "integrator:getAll"
 * @param payload 附带数据
 * @returns 后端返回的 data 字段
 */
export function sendMessage<T = unknown>(action: string, payload: unknown = {}): Promise<T> {
    return new Promise<T>((resolve, reject) => {
        const webview = (window as any).chrome?.webview
        if (!webview) {
            // 非 WebView2 环境：Mock 延迟返回，方便在浏览器中开发调试
            console.warn(`[bridge] 非 WebView2 环境，action="${action}" 将返回空数据`)
            resolve([] as T)
            return
        }

        const reqId = nextReqId()
        pending.set(reqId, {
            resolve: (data) => resolve(data as T),
            reject,
        })

        webview.postMessage(
            JSON.stringify({ action, reqId, payload })
        )
    })
}
