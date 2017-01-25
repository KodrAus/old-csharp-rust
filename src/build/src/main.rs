#[macro_use]
extern crate quick_error;
#[macro_use]
extern crate clap;
extern crate term_painter;

#[macro_use]
mod print;
mod args;
mod error;
mod artifact;
mod cargo;
mod msbuild;

use term_painter::ToStyle;
use term_painter::Color::*;
use clap::ArgMatches;
use args::FromArgs;

fn main() {
	let matches = args::app().get_matches();

    match build(matches) {
    	Ok(_) => {
    		println!("{}", Green.paint("The build finished successfully"));
    	},
    	Err(e) => {
    		println!("{}", Red.paint(e));
    		println!("\n{}", Red.bold().paint("The build did not finish successfully"));
    	}
    }
}

fn build(args: ArgMatches) -> Result<(), error::BuildError> {
	cargo::build(cargo::CargoBuildArgs::from_args(&args))?;

	artifact::copy(artifact::CopyArtifactArgs::from_args(&args))?;

	msbuild::build(msbuild::MsBuildArgs::from_args(&args))?;

	Ok(())
}
