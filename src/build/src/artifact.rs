use std::io::Error as IoError;
use std::fs;
use clap::ArgMatches;
use args::{FromArgs, BuildTarget};
use cargo::CargoPkg;
use msbuild::MsBuildPkg;

pub fn copy(args: CopyArtifactArgs) -> Result<(), CopyArtifactsError> {
	heading!("Copying rust artifacts", args);

	let rst_src = args.rst_src;
	let msbuild_dst = args.msbuild_dst;

	fs::copy(&rst_src, &msbuild_dst).map_err(|e| CopyArtifactsError::Io { src: rst_src, dst: msbuild_dst, err: e })?;

	Ok(())
}

#[derive(Debug, PartialEq)]
pub struct CopyArtifactArgs {
	rst_src: String,
	msbuild_dst: String
}

#[cfg(windows)]
const EXT: &'static str = "dll";

impl FromArgs for CopyArtifactArgs {
	fn from_args(args: &ArgMatches) -> Self {
		let target = BuildTarget::from_args(args);

		let CargoPkg(cargopkg) = CargoPkg::from_args(args);
		let MsBuildPkg(msbuildpkg) = MsBuildPkg::from_args(args);

		let target = match target {
			BuildTarget::Debug => "debug",
			BuildTarget::Release => "release"
		};

		let file = format!("{}.{}", cargopkg, EXT);

		let rst_src = format!("{}/target/{}/{}", cargopkg, target, file);
		let msbuild_dst = format!("{}/{}", msbuildpkg, file);

		CopyArtifactArgs {
			rst_src: rst_src,
			msbuild_dst: msbuild_dst
		}
	}
}

quick_error!{
	#[derive(Debug)]
	pub enum CopyArtifactsError {
		Io { src: String, dst: String, err: IoError } {
			cause(err)
			display("Error copying '{}' to '{}'\nCaused by: {}", src, dst, err)
		}
	}
}