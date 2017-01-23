extern crate libc;

use std::mem;
use libc::size_t;

#[repr(C)]
pub struct Buf {
	ptr: *mut u8,
	len: size_t,
	cap: size_t,
}

impl Buf {
	/// Build a `Buf` structure from a native `Vec`.
	/// 
	/// This will leak memory unless `to_vec` is called.
	fn from_vec(mut buf: Vec<u8>) -> Self {
		let ptr = buf.as_mut_ptr();
		let len = buf.len();
		let cap = buf.capacity();

		mem::forget(buf);

		Buf {
			ptr: ptr,
			len: len,
			cap: cap,
		}
	}

	/// Convert this `Buf` into a native `Vec`.
	/// 
	/// This will drop the value when it goes out of scope unless `from_vec` is called.
	unsafe fn to_vec(self) -> Vec<u8> {
		Vec::<u8>::from_raw_parts(self.ptr, self.len, self.cap)
	}
}

#[no_mangle]
pub extern fn alloc(size: size_t) -> Buf {
	println!("allocating buffer: {}", size);

	let buf = Vec::<u8>::with_capacity(size as usize);

	Buf::from_vec(buf)
}

#[no_mangle]
pub extern fn reserve(buf: Buf, reserve: size_t) -> Buf {
	println!("reserving {} extra bytes", reserve);

	let mut buf = unsafe { buf.to_vec() };

	buf.reserve(reserve);

	Buf::from_vec(buf)
}

#[no_mangle]
pub extern fn drop(buf: Buf) {
	let buf = unsafe { buf.to_vec() };

	println!("contains: {:?}", buf);
	println!("dropping buffer");
}
