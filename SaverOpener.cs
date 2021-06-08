using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Schema;
using System.Collections.ObjectModel;

namespace WPF2
{
    class SaverOpener
    {
        static public ObservableCollection<Circle> OpenFile(Stream file)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Library.CircleListType));
            Library.CircleListType lst;
            try
            {
                 lst = (Library.CircleListType)serializer.Deserialize(file);
            }
            catch (Exception)
            {
                (new WPF2.ErrorWindow()).ShowDialog();
                return null;
            }

            ObservableCollection<Circle> data = new ObservableCollection<Circle>();
            foreach (var c in lst.Circle)
                data.Add(new Circle() { Radius = c.radius, Frequency = c.frequency });

            return data;
        }

        static public void SaveFile(Stream file, ref ObservableCollection<Circle> data)
        {
            Library.CircleListType lst = new Library.CircleListType();
            lst.Circle = new Library.CircleType[data.Count];
            for (int i = 0; i < data.Count; i++)
            {
                lst.Circle[i] = new Library.CircleType();
                lst.Circle[i].radius = data[i].Radius;
                lst.Circle[i].frequency = data[i].Frequency;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(Library.CircleListType));
            serializer.Serialize(file, lst);
        }
    }
}
