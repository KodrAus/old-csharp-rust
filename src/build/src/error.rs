use cargo::CargoError;
use artifact::CopyArtifactsError;
use msbuild::MsBuildError;

quick_error!{
	#[derive(Debug)]
	pub enum BuildError {
		Rust(err: CargoError) {
			cause(err)
			display("Error building rust project\nCaused by: {}", err)
			from()
		}
		CopyArtifacts(err: CopyArtifactsError) {
			cause(err)
			display("Error copying artifacts\nCaused by: {}", err)
			from()
		}
		DotNet(err: MsBuildError) {
			cause(err)
			display("Error copying artifacts\nCaused by: {}", err)
			from()
		}
	}
}
