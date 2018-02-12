using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Pomutto
{
	public class FSPClient
	{
	    private UDPSocket m_UDPSocket;
	    private string m_Host;
	    private int m_Port;
		private bool m_IsRunning;
		
	    private Thread m_NetworkThread;
		
		public const int SOCKET_RECEIVE_INTERVAL = 10;
	    
		public bool Connect(string host, int port)
        {
            if (m_UDPSocket != null)
            {
                return false;
            }

//            Debuger.Log(LOG_TAG_MAIN, "Connect() 建立基础连接， host = {0}, port = {1}", (object)host, port);

            m_Host = host;
            m_Port = port;

            try
            {
//                m_HostEndPoint = UdpSocket.GetHostEndPoint(m_Host, m_Port);
//                if (m_HostEndPoint == null)
//                {
//                    Debuger.LogError(LOG_TAG_MAIN, "Connect() 无法将Host解析为IP！");
//                    Close();
//                    return false;
//                }
//                Debuger.Log(LOG_TAG_MAIN, "Connect() HostEndPoint = {0}", m_HostEndPoint.ToString());

                m_IsRunning = true;

                //创建Socket
//                Debuger.Log(LOG_TAG_MAIN, "Connect() 创建UdpSocket, AddressFamily = {0}", m_HostEndPoint.AddressFamily);
//                m_UDPSocket = CreateSocket(m_HostEndPoint.AddressFamily, m_Host, m_Port);
                

                //创建线程
//                Debuger.Log(LOG_TAG_MAIN, "Connect() 创建接收线程");
                m_NetworkThread = new Thread(NetworkThreadLoop) { IsBackground = true };
                m_NetworkThread.Start();

            }
            catch (Exception e)
            {
//                Debuger.LogError(LOG_TAG_MAIN, "Connect() " + e.Message + e.StackTrace);
                Disconnect();
                return false;
            }

            return true;
        }
		
		private void Disconnect()
		{
			if (m_NetworkThread != null)
			{
				m_NetworkThread.Interrupt();
				m_NetworkThread = null;
			}

			if (m_UDPSocket != null)
			{
				m_UDPSocket.Close();
				m_UDPSocket = null;
			}

			m_IsRunning = false;
		}

		private void NetworkThreadLoop()
		{
			while (m_IsRunning)
			{
				try
				{
					if (!DoReceive())
					{
						Thread.Sleep(SOCKET_RECEIVE_INTERVAL);
					}
				}
				catch (Exception e)
				{
					if (m_IsRunning)
					{
//						Debuger.LogError(LOG_TAG_RECV, "Thread_Receive() " + e.Message + "\n" + e.StackTrace);
						Thread.Sleep(500);
					}
				}
			}
		}

		private bool DoReceive()
		{
			byte[] buffer;
			m_UDPSocket.ReceiveBuffer(buffer);
			
		}
	}
}