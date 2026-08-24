pub fn add(left: i32, right: i32) -> i32 {
    left + right
}

#[unsafe(no_mangle)]
pub extern "system" fn my_function(value: i32) -> i32 {
    add(value, 1)
}
