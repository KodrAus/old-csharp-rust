# Rust/CSharp Interop

C# is capable of interopping with native C code. It's a common feature that projects like Kestrel use this heavily to communicate with `libuv` for efficient asynchronous io.

I'm not so keen on the idea of writing C though, I'd much rather write Rust. This repo is a playground for interop between Rust and C#, which can be done in the same way.

These samples use [`cargo-nuget`](https://github.com/KodrAus/cargo-nuget) for building and packaging the Rust libraries.

## What would be nice

- Share byte arrays between Rust and C#
- Poll a C# `Task` from Rust as a `Future`
- `await` a Rust `Future` in C#

## What to do

### Work out conventions between the Rust and C# ends

Who owns the memory? If it's allocated in Rust then it's freed in Rust, but who initiates that call? In Rust it's possible to invalidate a structure by moving it into a new shape (from `Vec<u8>` to `Buf`). So you simply require exclusive ownership of that instance. In C# we can't really do that, and once a buf has been sliced around it's not safe to dispose in Rust so long as those slices are still active.

So maybe we can look at the same ref-counting pattern used by `OwnedBuffer` in `corefxlab` and require that drops to 0 before returning ownership of the buffer to Rust. Or alternatively we can use Rust ref counting for all things and once that count drops to 0 from borrows in Rust or C# the buffer is dropped. That means a p/invoke call for each new reference though.