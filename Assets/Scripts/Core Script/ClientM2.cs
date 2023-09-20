using M2MqttUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt.Messages;
public class ClientM2 : M2MqttUnityClient
{

    public List<Environment_Struct.Message> eventMessages = new List<Environment_Struct.Message>();



    public void PublishMSG(string topicNamePub, string msgSend)
    {   
        if (client != null)
            client.Publish(topicNamePub, System.Text.Encoding.UTF8.GetBytes(msgSend), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);//-------------------------------
    }

    public List<Environment_Struct.Message> GetEventMsg() {

        if (eventMessages.Count > 0)
        {
            List<Environment_Struct.Message> msgs = new List<Environment_Struct.Message>(eventMessages);
            

            return msgs;

        }

        return null;
    }

    protected override void DecodeMessage(string topic, byte[] message)
    {
        StartCoroutine(ClearEventMessagesAfterDelay());

        Environment_Struct.Message holder = new Environment_Struct.Message();
        holder.topic = topic;
        holder.message = System.Text.Encoding.UTF8.GetString(message);
        Debug.Log(holder.message);
        eventMessages.Add(holder);        
    }


    private IEnumerator ClearEventMessagesAfterDelay()
    {
        yield return new WaitForSeconds(1.0f); // Wait for 1 second

        // Clear the eventMessages list after the delay
        eventMessages.Clear();
    }


    public void ConnectToServer()
    {
        base.Connect();
    }



    public void SubscribeTopics(string topicNameSub)
    {
        
       client.Subscribe(new string[] { topicNameSub }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });//------------------------------------------
    }

    void UnsubscribeTopics(string topicNameSub)
    {
        client.Unsubscribe(new string[] { topicNameSub });
    }
}


