using System;
using Unity.Sentis;
using UnityEditor.VersionControl;
using UnityEngine;

public class SentisGPUPixelTest : MonoBehaviour
{
    public ModelAsset modelAsset;
    public Texture2D inputTexture;

    private Worker _worker;
    private Tensor<float> _inputTensor;

    private TextureTransform _textureTransform;
    // private Tensor _outputTensor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Load the model
        Model runtimeModel = ModelLoader.Load(modelAsset);
        // Create a worker with the GPUPixel backend
        _worker = new Worker(runtimeModel, BackendType.GPUPixel);
        _textureTransform = new TextureTransform();
        Routine();
 }

    // Update is called once per frame
    [Obsolete("Obsolete")]
    async void Routine()
    {
        while (true)
        {
            _inputTensor = TextureConverter.ToTensor(inputTexture,_textureTransform);
            _worker.Schedule(_inputTensor);
            _inputTensor.Dispose();
            using var outputTensor = _worker.PeekOutput();
            using var cpuCopyTensor = await outputTensor.ReadbackAndCloneAsync();
            PrintLeak();
            await Awaitable.WaitForSecondsAsync(1);
        }
    }
    private void PrintLeak()
    {
        var allRenderTextures = Resources.FindObjectsOfTypeAll<RenderTexture>();
        Debug.Log($"memory leak : {allRenderTextures.Length}");
    }
    
}
