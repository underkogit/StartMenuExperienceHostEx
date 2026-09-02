use winapi::shared::windef::HWND;

use winapi::um::winuser::{SetWindowPos, HWND_BOTTOM, HWND_TOP, SWP_NOMOVE, SWP_NOSIZE};

#[unsafe(no_mangle)]
pub extern "system" fn set_window_zorder(hwnd: HWND, action: i32) -> i32 {
    let insert_after = if action == 0 { HWND_TOP } else { HWND_BOTTOM };
    unsafe { SetWindowPos(hwnd, insert_after, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE) }
}
