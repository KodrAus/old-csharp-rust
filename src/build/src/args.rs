use std::collections::HashMap;
use clap::{App, Arg, ArgMatches};
use cargo;
use msbuild;

const WATCH_ARG: &'static str = "watch";
const TEST_ARG: &'static str = "test";
const RELEASE_ARG: &'static str = "release";
const PLATFORM_ARG: &'static str = "platform";

pub fn app<'a, 'b>() -> App<'a, 'b> {
    App::new("Dotnet and Rust Build tool").args(&[Arg::with_name(WATCH_ARG)
                                                      .short("w")
                                                      .long("watch")
                                                      .help("run build on file changes"),
                                                  Arg::with_name(TEST_ARG)
                                                      .short("t")
                                                      .long("test")
                                                      .help("run cargo and dotnet tests"),
                                                  Arg::with_name(RELEASE_ARG)
                                                      .short("r")
                                                      .long("release")
                                                      .help("run an optimised build"),
                                                  Arg::with_name(PLATFORM_ARG)
                                                      .short("p")
                                                      .long("platform")
                                                      .multiple(true)
                                                      .takes_value(true)
                                                      .help("a platform to target"),
                                                  cargo::pkg_arg(),
                                                  msbuild::pkg_arg()])
}

pub trait FromArgs {
    fn from_args(args: &ArgMatches) -> Self;
}

#[derive(Debug, PartialEq)]
pub enum BuildKind {
    Build,
    Test,
}

impl FromArgs for BuildKind {
    fn from_args(args: &ArgMatches) -> Self {
        match args.is_present(TEST_ARG) {
            true => BuildKind::Test,
            _ => BuildKind::Build,
        }
    }
}

#[derive(Debug, PartialEq)]
pub enum BuildTarget {
    Debug,
    Release,
}

impl FromArgs for BuildTarget {
    fn from_args(args: &ArgMatches) -> Self {
        match args.is_present(RELEASE_ARG) {
            true => BuildTarget::Release,
            _ => BuildTarget::Debug,
        }
    }
}

#[derive(Debug, Clone, PartialEq)]
pub enum BuildPlatform {
    Linux,
    Windows,
    Osx,
}

#[cfg(windows)]
impl Default for BuildPlatform {
    fn default() -> Self {
        BuildPlatform::Windows
    }
}

#[cfg(macos)]
impl Default for BuildPlatform {
    fn default() -> Self {
        BuildPlatform::Osx
    }
}

#[cfg(unix)]
impl Default for BuildPlatform {
    fn default() -> Self {
        BuildPlatform::Linux
    }
}

impl FromArgs for Vec<BuildPlatform> {
    fn from_args(args: &ArgMatches) -> Self {
        let mut results = HashMap::new();

        if let Some(args) = args.values_of(PLATFORM_ARG) {
            for arg in args {
                let value = match arg {
                    "linux" => Some(BuildPlatform::Linux),
                    "windows" => Some(BuildPlatform::Windows),
                    "osx" => Some(BuildPlatform::Osx),
                    _ => None
                };

                if let Some(value) = value {
                    results.insert(arg, value);
                }
            }
        }

        let len = results.values().len();

        match len {
            0 => vec![BuildPlatform::default()],
            _ => results.values().cloned().collect()
        }
    }
}