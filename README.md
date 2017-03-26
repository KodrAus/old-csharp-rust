# Rust/CSharp Interop

C# is capable of interopping with native C code. It's a common feature that projects like Kestrel use this heavily to communicate with `libuv` for efficient asynchronous io.

I'm not so keen on the idea of writing C though, I'd much rather write Rust. This repo is a playground for interop between Rust and C#, which can be done in the same way.

These samples use [`cargo-nuget`](https://github.com/KodrAus/cargo-nuget) for building and packaging the Rust libraries.

## What would be nice

- Share byte arrays between Rust and C#
- Poll a C# `Task` from Rust as a `Future`