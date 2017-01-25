use std::process::{Command, Stdio};
use std::io::Error as IoError;
use clap::{Arg, ArgMatches};
use args::{FromArgs, BuildKind, BuildTarget, BuildPlatform};

pub fn build(args: CargoBuildArgs) -> Result<(), CargoError> {
    heading!("Building rust project", args);

    let mut cargo = Command::new("cargo");

    cargo.current_dir(&args.work_dir);
    cargo.stdout(Stdio::inherit());
    cargo.stderr(Stdio::inherit());

    cargo.arg(match args.kind {
        BuildKind::Build => "build",
        BuildKind::Test => "test",
    });

    if args.platform != BuildPlatform::default() {
        let platform = match args.platform {
            BuildPlatform::Windows => "x86_64-pc-windows-gnu",
            BuildPlatform::Osx => "x86_64-apple-darwin",
            BuildPlatform::Linux => "x86_64-unknown-linux-gnu",
        };

        cargo.arg(format!("--target={}", platform));
    }

    if args.target == BuildTarget::Release {
        cargo.arg("--release");
    }

    let output = cargo.output().map_err(|e| CargoError::from(e))?;

    if !output.status.success() {
        Err(CargoError::Run)?;
    }

    Ok(())
}

pub const CARGOPKG_ARG: &'static str = "cargo-pkg";

pub fn pkg_arg<'a, 'b>() -> Arg<'a, 'b> {
    Arg::with_name(CARGOPKG_ARG).default_value("native").help("name of the rust library")
}

#[derive(Debug, PartialEq)]
pub struct CargoBuildArgs {
    pub work_dir: String,
    pub kind: BuildKind,
    pub target: BuildTarget,
    pub platform: BuildPlatform,
}

impl FromArgs for CargoBuildArgs {
    fn from_args(args: &ArgMatches) -> Self {
        let CargoPkg(work_dir) = CargoPkg::from_args(args);
        let platforms = Vec::<BuildPlatform>::from_args(args);

        // TODO: Support multiple platforms
        let platform = platforms.into_iter().next().unwrap_or(BuildPlatform::default());

        CargoBuildArgs {
            work_dir: work_dir,
            kind: BuildKind::from_args(args),
            target: BuildTarget::from_args(args),
            platform: platform,
        }
    }
}

#[derive(Debug, PartialEq)]
pub struct CargoPkg(pub String);

impl AsRef<str> for CargoPkg {
    fn as_ref(&self) -> &str {
        &self.0
    }
}

impl FromArgs for CargoPkg {
    fn from_args(args: &ArgMatches) -> Self {
        CargoPkg(args.value_of(CARGOPKG_ARG).expect("should have default value").into())
    }
}

quick_error!{
	#[derive(Debug)]
	pub enum CargoError {
		Io(err: IoError) {
			cause(err)
			display("Error running 'cargo' command\nCaused by: {}", err)
			from()
		}
		Run {
			display("Cargo build error")
		}
	}
}
