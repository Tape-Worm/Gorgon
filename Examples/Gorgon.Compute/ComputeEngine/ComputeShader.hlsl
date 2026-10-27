// A simple data buffer holding some data.
struct DataBuffer
{
	int Sum;
	float Product;
};

// Our inputs and outputs.
Buffer<int> IntData : register(t0);
Buffer<float> FltData : register(t1);
RWStructuredBuffer<DataBuffer> OutputData : register(u0);

// A -very- simple compute shader to show how to use the compute engine in Gorgon.
[numthreads(1, 1, 1)]
void SimpleCompute(uint3 threadId : SV_DispatchThreadID)
{
	OutputData[threadId.x].Sum = IntData[threadId.x] + IntData[threadId.x];
	OutputData[threadId.x].Product = FltData[threadId.x] * FltData[threadId.x];
}