#[macro_export]
macro_rules! heading {
    ($line:expr, $args:ident) => ({
    	use term_painter::ToStyle;
		use term_painter::Color::*;

    	println!("{}", Cyan.bold().paint(format!("\n\n{}\n\nUsing args: {:?}\n", $line, $args)));
    })
}