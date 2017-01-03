using System;
using System.Runtime.InteropServices;

class Native
{
    // A simple external Rust function
    [DllImport("native", CallingConvention = CallingConvention.Cdecl)]
    static extern void hello_from_rust(int a);

    // Call `hello_from_rust`
    public void HelloFromRust(int a)
    {
        hello_from_rust(a);
    }
    
    // An external Rust function that'll execute a fn pointer
    [DllImport("native", CallingConvention = CallingConvention.Cdecl)]
    static extern void say_hello(say_hello_cb cb);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate void say_hello_cb(int a);

    // Call `say_hello`
    public void SayHello()
    {
        say_hello(a =>
        {
            Console.WriteLine($"Hello from C#: {a}");
        });
    }
}

class Program
{
    static void Main(string[] args)
    {
        var native = new Native();

        native.HelloFromRust(5);
        native.SayHello();
    }
}
