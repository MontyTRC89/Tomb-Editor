// Cross-compiler binding annotations.
//
// FXC (HLSL -> DXBC for DX11) does not understand [[vk::*]] attributes and
// raises a syntax error on them. DXC compiles fine in both DXBC and SPIR-V
// modes but only honors them in SPIR-V mode. The build pipeline passes
// `-D SPIRV` only to the DXC SPIR-V invocation, so these macros expand to
// the real attributes only there and disappear everywhere else.
//
// Convention:
//   * Each cbuffer / texture / sampler should pin its slot with VK_BINDING.
//   * Each VS input attribute should pin its location with VK_LOCATION.
//   * The pixel shader's color output is bound automatically via SV_Target.

// Parameter names are intentionally underscore-prefixed: the C preprocessor
// is unaware of HLSL scoping, so a parameter named `binding` would otherwise
// collide with the `binding` identifier in `vk::binding` during text
// substitution and turn the expansion into garbage.
#ifdef SPIRV
  #define VK_BINDING(_b, _s)  [[vk::binding(_b, _s)]]
  #define VK_LOCATION(_l)     [[vk::location(_l)]]
  #define VK_PUSH_CONSTANT    [[vk::push_constant]]
#else
  #define VK_BINDING(_b, _s)
  #define VK_LOCATION(_l)
  #define VK_PUSH_CONSTANT
#endif
