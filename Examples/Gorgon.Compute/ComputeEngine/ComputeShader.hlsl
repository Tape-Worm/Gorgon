// Our inputs and outputs.
struct ComputeData
{
	int intBufferHandle;
	int floatBufferHandle;
	int outputBufferHandle;
	int padding;
};

// A simple data buffer holding some data.
struct DataBuffer
{
	int Sum;
	float Product;
};

ConstantBuffer<ComputeData> _computeData : register(b0);

// A -very- simple compute shader to show how to use the compute engine in Gorgon.
[numthreads(64, 1, 1)]
void SimpleCompute(uint3 threadId : SV_DispatchThreadID)
{
	Buffer<int> intData = ResourceDescriptorHeap[_computeData.intBufferHandle];
	Buffer<float> fltData = ResourceDescriptorHeap[_computeData.floatBufferHandle];
	RWStructuredBuffer<DataBuffer> outputData = ResourceDescriptorHeap[_computeData.outputBufferHandle];

	outputData[threadId.x].Sum = intData[threadId.x] + intData[threadId.x];
	outputData[threadId.x].Product = fltData[threadId.x] * fltData[threadId.x];
}