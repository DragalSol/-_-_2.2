using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

public class Item
{
    public string Title { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class Order
{
    public class ShipTo
    {
        public string Name { get; set; }
        public string Street { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
    }

    public ShipTo ShipInfo { get; set; }
    public List<Item> Items { get; set; }
}

public static class XmlToOrderConverter
{
    public static Order ConvertXmlToOrder(string xmlString)
    {
        XElement xmlDoc = XElement.Parse(xmlString);

        var order = new Order
        {
            ShipInfo = xmlDoc.Element("shipTo")?.Elements()
                .ToDictionary(e => e.Name.LocalName, e => e.Value)
                .ToShipTo(),

            Items = xmlDoc.Descendants("item")
                .Select(itemNode => new Item
                {
                    Title = itemNode.Element("title")?.Value,
                    Quantity = Convert.ToInt32(itemNode.Element("quantity")?.Value),
                    Price = Convert.ToDecimal(itemNode.Element("price")?.Value.Replace('.', ','))
                })
                .ToList()
        };

        return order;
    }

    private static Order.ShipTo ToShipTo(this Dictionary<string, string> dictionary)
    {
        return new Order.ShipTo
        {
            Name = dictionary.GetValueOrDefault("name"),
            Street = dictionary.GetValueOrDefault("street"),
            Address = dictionary.GetValueOrDefault("address"),
            Country = dictionary.GetValueOrDefault("country")
        };
    }

    private static string GetValueOrDefault(this Dictionary<string, string> dictionary, string key)
    {
        return dictionary.TryGetValue(key, out var value) ? value : null;
    }
}

class Program
{
    static void Main()
    {
        string xmlString = @"
            <shipOrder>
                <shipTo>
                    <name>Tove Svendson</name>
                    <street>Ragnhildvei 2</street>
                    <address>4000 Stavanger</address>
                    <country>Norway</country>
                </shipTo>
                <items>
                    <item>
                        <title>Empire Burlesque</title>
                        <quantity>1</quantity>
                        <price>10.90</price>
                    </item>
                    <item>
                        <title>Hide your heart</title>
                        <quantity>1</quantity>
                        <price>9.90</price>
                    </item>
                </items>
            </shipOrder>";

        Order order = XmlToOrderConverter.ConvertXmlToOrder(xmlString);

        Console.WriteLine("Информация о доставке:");
        Console.WriteLine($"Имя: {order.ShipInfo?.Name}");
        Console.WriteLine($"Улица: {order.ShipInfo?.Street}");
        Console.WriteLine($"Адрес: {order.ShipInfo?.Address}");
        Console.WriteLine($"Страна: {order.ShipInfo?.Country}");

        Console.WriteLine("\nТовары в заказе:");
        foreach (Item item in order.Items)
        {
            Console.WriteLine($"Наименование: {item.Title}, Количество: {item.Quantity}, Цена: {item.Price:C}");
        }
    }
}
