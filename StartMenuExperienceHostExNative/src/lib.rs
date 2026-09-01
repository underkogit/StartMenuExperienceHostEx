use winapi::shared::windef::HWND;

use winapi::um::winuser::{
    CallNextHookEx, DispatchMessageW, GetMessageW, HC_ACTION, HWND_BOTTOM, HWND_TOP,
    KBDLLHOOKSTRUCT, MSG, PostQuitMessage, SWP_NOMOVE, SWP_NOSIZE, SetWindowPos, SetWindowsHookExW,
    TranslateMessage, UnhookWindowsHookEx, WH_KEYBOARD_LL, WM_KEYDOWN, WM_KEYUP,
};

#[unsafe(no_mangle)]
pub extern "system" fn set_window_zorder(hwnd: HWND, action: i32) -> i32 {
    let insert_after = if action == 0 { HWND_TOP } else { HWND_BOTTOM };
    unsafe { SetWindowPos(hwnd, insert_after, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE) }
}
