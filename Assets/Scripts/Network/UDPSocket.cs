using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Pomutto
{
	public class UDPSocket : IDisposable
	{
		private AddressFamily m_AddressFamily;
		private Socket m_Socket;
		private byte[] m_Buffer;

		private const int BUFFER_MAX_SIZE = 256;
		
		public UDPSocket(AddressFamily family) 
		{
			m_AddressFamily = family;
			m_Socket = new Socket(m_AddressFamily, SocketType.Dgram, ProtocolType.Udp);
			IPEndPoint ipep = GetIPEndPointAny(m_AddressFamily, 0);
			m_Socket.Bind(ipep);

			m_Buffer = new byte[BUFFER_MAX_SIZE];
		}
		
		public void Dispose()
		{
			Close();
		}

		public void Close()
		{
			if (m_Socket != null)
			{
				try
				{
					m_Socket.Shutdown(SocketShutdown.Both);
				}
				catch (Exception e)
				{
//					Debuger.LogWarning(TAG, "Close() " + e.Message + e.StackTrace);
				}

				m_Socket.Close();
				m_Socket = null;
			}
			GC.SuppressFinalize(this);
		}
		
		public static IPEndPoint GetIPEndPointAny(AddressFamily family, int port)
		{
			if (family == AddressFamily.InterNetwork)
			{
				if (port == 0)
				{
					return new IPEndPoint(IPAddress.Any, 0);
				}
				return new IPEndPoint(IPAddress.Any, port);
			}
			return null;
		}

		public int ReceiveBuffer(byte[] buffer)
		{
			EndPoint ip = null;
			int bufferEnd = m_Socket.ReceiveFrom(m_Buffer, BUFFER_MAX_SIZE, SocketFlags.None, ref ip);
			return bufferEnd;
		}
	}
}