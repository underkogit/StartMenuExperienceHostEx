use winapi::shared::windef::HWND;

use winapi::um::winuser::{HWND_BOTTOM, HWND_TOP, SWP_NOMOVE, SWP_NOSIZE, SetWindowPos, DestroyIcon};

use std::ffi::OsStr;
use std::os::windows::ffi::OsStrExt;
use std::path::Path;
use std::mem;

use winapi::um::shellapi::{
    SHGetFileInfoW, SHGFI_ICON, SHGFI_LARGEICON, SHGFI_SHELLICONSIZE
};
use winapi::shared::minwindef::DWORD;
use winapi::shared::windef::HICON;

#[unsafe(no_mangle)]
pub extern "system" fn set_window_zorder(hwnd: HWND, action: i32) -> i32 {
    let insert_after = if action == 0 { HWND_TOP } else { HWND_BOTTOM };
    unsafe { SetWindowPos(hwnd, insert_after, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE) }
}

#[repr(C)]
struct SHFILEINFOW {
    h_icon: HICON,
    i_icon: i32,
    dw_attributes: DWORD,
    sz_display_name: [u16; 260],
    sz_type_name: [u16; 80],
}

#[unsafe(no_mangle)]
pub extern "system" fn get_file_icon_max_quality(file_path: *const u16) -> *mut u16 {
    if file_path.is_null() {
        return std::ptr::null_mut();
    }

    let mut file_info: SHFILEINFOW = unsafe { mem::zeroed() };
    let size = mem::size_of::<SHFILEINFOW>() as u32;

    let flags = SHGFI_ICON | SHGFI_LARGEICON | SHGFI_SHELLICONSIZE;

    let result = unsafe {
        SHGetFileInfoW(
            file_path,
            0x80,
            &mut file_info as *mut _ as *mut _,
            size,
            flags,
        )
    };

    if result == 0 || file_info.h_icon.is_null() {
        return std::ptr::null_mut();
    }

    let file_name = unsafe {
        let len = (0..260).take_while(|&i| *file_path.offset(i) != 0).count();
        let wide_path = std::slice::from_raw_parts(file_path, len);
        let path = String::from_utf16_lossy(wide_path);
        Path::new(&path)
            .file_stem()
            .and_then(|s| s.to_str())
            .unwrap_or("icon")
            .to_string()
    };

    let icon_dir = "./icons";
    let _ = std::fs::create_dir_all(icon_dir);

    let icon_path = format!("{}/{}.ico", icon_dir, file_name);

    let wide_icon_path: Vec<u16> = OsStr::new(&icon_path)
        .encode_wide()
        .chain(Some(0))
        .collect();

    let result_ptr = wide_icon_path.as_ptr() as *mut u16;
    std::mem::forget(wide_icon_path);

    unsafe {
        DestroyIcon(file_info.h_icon);
    }

    result_ptr
}

#[unsafe(no_mangle)]
pub extern "system" fn free_string(ptr: *mut u16) {
    if !ptr.is_null() {
        unsafe {
            let len = (0..).take_while(|&i| *ptr.offset(i) != 0).count();
            let _ = Vec::from_raw_parts(ptr, len, len);
        }
    }
}