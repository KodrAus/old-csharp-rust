use clap::{App, Arg, ArgMatches};
use cargo;
use msbuild;

const WATCH_ARG: &'static str = "watch";
const TEST_ARG: &'static str = "test";
const RELEASE_ARG: &'static str = "release";

pub fn app<'a, 'b>() -> App<'a, 'b> {
	App::new("Dotnet and Rust Build tool")
		.args(&[
			Arg::with_name(WATCH_ARG).short("w").long("watch").help("run build on file changes"),
			Arg::with_name(TEST_ARG).short("t").long("test").help("run cargo and dotnet tests"),
			Arg::with_name(RELEASE_ARG).short("r").long("release").help("run an optimised build"),
			cargo::pkg_arg(),
			msbuild::pkg_arg(),
		])
}

pub trait FromArgs {
	fn from_args(args: &ArgMatches) -> Self;
}

#[derive(Debug, PartialEq)]
pub enum BuildKind {
	Build,
	Test
}

impl FromArgs for BuildKind {
	fn from_args(args: &ArgMatches) -> Self {
		match args.is_present(TEST_ARG) {
			true => BuildKind::Test,
			_ => BuildKind::Build
		}
	}
}

#[derive(Debug, PartialEq)]
pub enum BuildTarget {
	Debug,
	Release
}

impl FromArgs for BuildTarget {
	fn from_args(args: &ArgMatches) -> Self {
		match args.is_present(RELEASE_ARG) {
			true => BuildTarget::Release,
			_ => BuildTarget::Debug
		}
	}
}
