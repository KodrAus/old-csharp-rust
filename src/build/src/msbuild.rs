use std::process::{Command, Stdio};
use std::io::Error as IoError;
use clap::{Arg, ArgMatches};
use args::{FromArgs, BuildKind, BuildTarget};

pub fn build(args: MsBuildArgs) -> Result<(), MsBuildError> {
    heading!("Building dotnet project", args);

    let output = Command::new("dotnet").arg("build")
        .current_dir(&args.work_dir)
        .stdout(Stdio::inherit())
        .stderr(Stdio::inherit())
        .output()
        .map_err(|e| MsBuildError::from(e))?;

    if !output.status.success() {
        Err(MsBuildError::Run)?
    }

    Ok(())
}

pub const MSBUILDPKG_ARG: &'static str = "msbuild-pkg";

pub fn pkg_arg<'a, 'b>() -> Arg<'a, 'b> {
    Arg::with_name(MSBUILDPKG_ARG).default_value("dotnet").help("name of the dotnet library")
}

#[derive(Debug, PartialEq)]
pub struct MsBuildArgs {
    pub work_dir: String,
    pub kind: BuildKind,
    pub target: BuildTarget,
}

impl FromArgs for MsBuildArgs {
    fn from_args(args: &ArgMatches) -> Self {
        let MsBuildPkg(work_dir) = MsBuildPkg::from_args(args);

        MsBuildArgs {
            work_dir: work_dir,
            kind: BuildKind::from_args(args),
            target: BuildTarget::from_args(args),
        }
    }
}

#[derive(Debug, PartialEq)]
pub struct MsBuildPkg(pub String);

impl AsRef<str> for MsBuildPkg {
    fn as_ref(&self) -> &str {
        &self.0
    }
}

impl FromArgs for MsBuildPkg {
    fn from_args(args: &ArgMatches) -> Self {
        MsBuildPkg(args.value_of(MSBUILDPKG_ARG).expect("should have default value").into())
    }
}

quick_error!{
	#[derive(Debug)]
	pub enum MsBuildError {
		Io(err: IoError) {
			cause(err)
			display("Error running 'dotnet' command\nCaused by: {}", err)
			from()
		}
		Run {
			display("MsBuild build error")
		}
	}
}
