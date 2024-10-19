
// VERTEX SHADER
struct UniformBuffer {
    mdlMat : mat4x4<f32>
};

struct Input
{
	@location(0) pos : vec4<f32>,
};

struct Output 
{
    @builtin(position) pos : vec4<f32>,
};

@group(0)
@binding(0)
var<uniform> ub : UniformBuffer;

// VERTEX ENTRY
@vertex
fn vs_main(
    in: Input
	) -> Output 
{
    return Output(ub.mdlMat * vec4<f32>(in.pos.xyz, 1.0));
}

// FRAGMENT ENTRY
@fragment
fn fs_main(in : Output) 
    -> @location(0) vec4<f32> 
{
    return vec4<f32>(1.0, 0.0, 1.0, 1.0);
}