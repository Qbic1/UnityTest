using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MotionController : MonoBehaviour {
	public GameObject TransmitterPrefab;
	public GameObject ReceiverPrefab;

	private List<GameObject> Receivers = new List<GameObject> ();
	private GameObject Transmitter;
	private Transmitter _transmitter;
	private float time = 0;
	private int curPos = 0;
	private Vector3 pos;
	private string dataPath;

	// Use this for initialization
	void Awake () {
		dataPath = Application.dataPath + "/Data/output.txt";
		if (File.Exists ("Assets/Data/input.txt")) {
			StreamReader input = File.OpenText ("Assets/Data/input.txt");
			var positions = input.ReadLine ().Split(',');
			var times = new List<string>();
			while (!input.EndOfStream) {
				times.AddRange(input.ReadLine ().Split (','));
			}
			for (int i = 0; i < 3; i++) {
				InstReceiver (float.Parse(positions [2*i]), float.Parse(positions [2*i+1]), times, i);
			}
			InstTransmitter ();
			PrintToFile ();
		}
	}

	void PrintToFile()
	{
		if (!File.Exists(dataPath)){
			var dataWriter = new StreamWriter(dataPath);
			for (int i=0; i < _transmitter.X.Count; i++)
				dataWriter.WriteLine(_transmitter.X[i] + "," + _transmitter.Y[i]);
			dataWriter.Flush ();
			dataWriter.Close ();
		}
	}

	void InstReceiver(float x, float y, List<string> time, int num) {
		GameObject temp = Instantiate(ReceiverPrefab);
		temp.name = "Reciever" + num;
		Receiver _receiver = temp.GetComponent<Receiver> ();
		_receiver.X = x;
		_receiver.Y = y;
		_receiver.Time = new List<float>();
		for (int i = num; i < time.Count; i += 3) {
			_receiver.Time.Add (float.Parse(time [i]));
		}
		_receiver.CalculateDist ();
		temp.transform.position = new Vector3 (x, y, 0);
		Receivers.Add (temp);
	}

	void InstTransmitter()
	{
		List<float> resX = new List<float>(), resY = new List<float>();
		float a, dx, dy, d, h, rx, ry;
		float point2_x, point2_y;

		Receiver _rec1 = Receivers [0].GetComponent<Receiver> ();
		Receiver _rec2 = Receivers [1].GetComponent<Receiver> ();
		Receiver _rec3 = Receivers [2].GetComponent<Receiver> ();

		for (int i = 0; i < _rec1.Dist.Count; i++) {
			// считаем разницу координат для 2 точек
			dx = _rec2.X - _rec1.X;
			dy = _rec2.Y - _rec1.Y;

			// расстояние между центрами
			d = Mathf.Sqrt ((dy * dy) + (dx * dx));
			a = ((_rec1.Dist [i] * _rec1.Dist [i]) - (_rec2.Dist [i] * _rec2.Dist [i]) + (d * d)) / (2 * d);

			point2_x = _rec1.X + (dx * a / d);
			point2_y = _rec1.Y + (dy * a / d);
			h = Mathf.Sqrt (Mathf.Abs((_rec1.Dist [i] * _rec1.Dist [i]) - (a * a)));
			rx = -dy * (h / d);
			ry = dx * (h / d);

			float intersectionPoint1_x = point2_x + rx;
			float intersectionPoint2_x = point2_x - rx;
			float intersectionPoint1_y = point2_y + ry;
			float intersectionPoint2_y = point2_y - ry;

			dx = intersectionPoint1_x - _rec3.X;
			dy = intersectionPoint1_y - _rec3.Y;
			float d1 = Mathf.Sqrt ((dy * dy) + (dx * dx));

			dx = intersectionPoint2_x - _rec3.X;
			dy = intersectionPoint2_y - _rec3.Y;
			float d2 = Mathf.Sqrt ((dy * dy) + (dx * dx));

			if (Mathf.Abs (d1 - _rec3.Dist [i]) < Mathf.Abs (d2 - _rec3.Dist [i])) {
				resX.Add (intersectionPoint1_x);
				resY.Add (intersectionPoint1_y);
			} else {
				resX.Add (intersectionPoint2_x);
				resY.Add (intersectionPoint2_y);
			}
		}

		GameObject temp = Instantiate (TransmitterPrefab);
		temp.name = "Transmitter";
		_transmitter = temp.GetComponent<Transmitter> ();
		_transmitter.X = resX;
		_transmitter.Y = resY;
		temp.transform.position = new Vector3 (resX [0], resY [0], 0);
		Transmitter = temp;
		pos = temp.transform.position;
	}

	// Update is called once per frame
	void Update () {
		if (time > 1) {
			curPos += 1;
			if (curPos < _transmitter.X.Count) {
				Transmitter.transform.position = new Vector3 (_transmitter.X [curPos], _transmitter.Y [curPos], 0);
				pos = Transmitter.transform.position;
			}
			time = 0;
		} else {
			if (curPos < _transmitter.X.Count - 1)
				Transmitter.transform.position = Vector3.Lerp (pos, 
					new Vector3 (_transmitter.X [curPos + 1], _transmitter.Y [curPos + 1], 0), time);
			time += Time.deltaTime;
		}
	}
}
