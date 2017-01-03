#[no_mangle]
pub extern fn hello_from_rust(a: i32) {
	println!("Hello from Rust: {}", a);
}

#[no_mangle]
pub extern fn say_hello(cb: extern fn(a: i32)) {
	cb(7);
}