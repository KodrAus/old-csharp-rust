extern crate slab;
extern crate pool;

use slab::Slab;
use pool::{Pool, Dirty, Checkout};

// TODO: Make this thread safe
struct MemPool {
	checkouts: Slab<Checkout<Dirty<()>>>,
	buffer: Pool<Dirty<()>>
}

#[cfg(test)]
mod tests {
	use slab::Slab;
	use pool::{Pool, Dirty};
	use super::MemPool;

	#[test]
	fn it_works() {
		// Create a memory pool
		let mut pool = MemPool {
			checkouts: Slab::with_capacity(1),
			buffer: Pool::with_capacity(1, 4, || Dirty(()))
		};

		// Get an entry from the pool, store a handle to it and return its index and pointer
		let (idx, ptr) = {
			let buf = pool.buffer.checkout().unwrap();
			let mut entry = pool.checkouts
				.vacant_entry()
				.unwrap()
				.insert(buf);
				
			let ptr = entry
				.get_mut()
				.extra_mut()
				.as_mut_ptr();

			(entry.index(), ptr)
		};

		// Someone writing to this memory from C#
		{
			let mut slice = unsafe { ptr.as_mut().unwrap() };
			*slice = 1;
		}

		// The pool is now full so we can't checkout anymore
		assert!(pool.buffer.checkout().is_none());

		// Remove the entry and let it fall out of scope
		{
			let _ = pool.checkouts.remove(idx).unwrap();
		}

		// The pool is not full so we can checkout again
		let entry = pool.buffer.checkout().unwrap();

		let bytes = entry.extra();
		assert_eq!(&[1,0,0,0,0,0,0,0], bytes);
	}
}