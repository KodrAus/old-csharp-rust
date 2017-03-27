extern crate libc;

use std::mem;
use libc::size_t;

#[repr(C)]
pub struct ResizeBuf {
	ptr: *mut u8,
	len: size_t,
	cap: size_t,
}

impl ResizeBuf {
	/// Build a `ResizeBuf` structure from a native `Vec`.
	/// 
	/// This will leak memory unless `to_vec` is called.
	fn from_vec(mut buf: Vec<u8>) -> Self {
		let ptr = buf.as_mut_ptr();
		let len = buf.len();
		let cap = buf.capacity();

		mem::forget(buf);

		ResizeBuf {
			ptr: ptr,
			len: len,
			cap: cap,
		}
	}

	/// Convert this `ResizeBuf` into a native `Vec`.
	/// 
	/// This will drop the value when it goes out of scope unless `from_vec` is called.
	unsafe fn to_vec(self) -> Vec<u8> {
		Vec::<u8>::from_raw_parts(self.ptr, self.len, self.cap)
	}
}

#[no_mangle]
pub extern fn alloc(size: size_t) -> ResizeBuf {
	println!("allocating buffer: {}", size);

	let buf = Vec::<u8>::with_capacity(size as usize);

	ResizeBuf::from_vec(buf)
}

#[no_mangle]
pub extern fn reserve(buf: ResizeBuf, reserve: size_t) -> ResizeBuf {
	println!("reserving {} extra bytes", reserve);

	let mut buf = unsafe { buf.to_vec() };

	buf.reserve(reserve);

	ResizeBuf::from_vec(buf)
}

#[no_mangle]
pub extern fn drop(buf: ResizeBuf) {
	let buf = unsafe { buf.to_vec() };

	println!("contains: {:?}", buf);
	println!("dropping buffer");
}
