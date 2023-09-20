using System;
using UnityEngine;
using RosH;





public class ImagePublisher : MonoBehaviour
{
    public ClientM2 clientClass;
    public Camera cameraToPublish;
    private Texture2D unityTexture;
    



    private void Start()
    {
        StartCoroutine(PublishImageEverySecond());
    }

    private System.Collections.IEnumerator PublishImageEverySecond()
    {
        while (true)
        {
            yield return new WaitForSeconds(2.0f);
            unityTexture = CaptureCameraImage(cameraToPublish);

            if (unityTexture != null)
            {
                PublishImage();
            }
        }
    }


    private Texture2D CaptureCameraImage(Camera camera)
    {
        RenderTexture renderTexture = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 24);
        camera.targetTexture = renderTexture;
        camera.Render();
        RenderTexture.active = renderTexture;

        Texture2D image = new Texture2D(camera.pixelWidth, camera.pixelHeight);
        image.ReadPixels(new Rect(0, 0, camera.pixelWidth, camera.pixelHeight), 0, 0);
        image.Apply();

        camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        return image;
    }

    private void PublishImage()
    {
        // Create the image message
        Image imageMessage = CreateImageMessage();

        // Serialize the image message to JSON
        string jsonMessage = JsonUtility.ToJson(imageMessage);

        clientClass.PublishMSG("Cam", jsonMessage);
        // Convert JSON message to bytes
        //byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(jsonMessage);

        // Publish the serialized JSON message over MQTT
        //mqttClient.Publish(mqttTopic, messageBytes);
    }

    private Image CreateImageMessage()
    {
        // Create and fill the Image message
        Image imageMessage = new Image();
        // ... (fill in header, height, width, encoding, etc.)
        // For example:
        imageMessage.header = new Header();
        imageMessage.height = (uint)unityTexture.height;
        imageMessage.width = (uint)unityTexture.width;
        imageMessage.encoding = "rgb8";
        imageMessage.is_bigendian = 0;
        imageMessage.step = (uint)(unityTexture.width * 3);
        imageMessage.data = EncodeTextureData(unityTexture);
        
        return imageMessage;
    }


    private byte[] EncodeTextureData(Texture2D texture)
    {
        // Encode the texture data based on the encoding
        // You need to implement this part based on your specific encoding needs
        // For example, for "rgb8" encoding, you might convert RGB values to a byte array

        // Replace this with actual encoding logic
        Color[] pixels = texture.GetPixels();
        byte[] encodedData = new byte[pixels.Length * 3]; // 3 channels (RGB)

        for (int i = 0; i < pixels.Length; i++)
        {
            encodedData[i * 3] = (byte)(pixels[i].r * 255);
            encodedData[i * 3 + 1] = (byte)(pixels[i].g * 255);
            encodedData[i * 3 + 2] = (byte)(pixels[i].b * 255);
        }

        return encodedData;
    }
}
