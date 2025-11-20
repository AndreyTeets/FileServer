using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;

namespace FileServer.Configuration;

internal sealed class InMemoryXmlRepository : IDeletableXmlRepository
{   // Simplified copy of Microsoft.AspNetCore.DataProtection.Repositories.EphemeralXmlRepository
    private readonly List<XElement> _storedElements = [];

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        lock (_storedElements)
            return _storedElements.ConvertAll(x => new XElement(x)).AsReadOnly();
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        lock (_storedElements)
            _storedElements.Add(new XElement(element));
    }

    public bool DeleteElements(Action<IReadOnlyCollection<IDeletableElement>> chooseElements)
    {
        lock (_storedElements)
        {
            List<DeletableElement> deletable = _storedElements.ConvertAll(x => new DeletableElement(x, new XElement(x)));
            chooseElements(deletable);
            foreach (DeletableElement de in deletable.Where(e => e.DeletionOrder.HasValue))
                _storedElements.Remove(de.StoredElement);
        }
        return true;
    }

    private sealed class DeletableElement(XElement storedElement, XElement element) : IDeletableElement
    {
        internal XElement StoredElement { get; } = storedElement;
        public XElement Element { get; } = element;
        public int? DeletionOrder { get; set; }
    }

    internal sealed class NoopXmlEncryptor : IXmlEncryptor
    {
        public EncryptedXmlInfo Encrypt(XElement plaintextElement) => new(plaintextElement, typeof(NoopXmlDecryptor));
    }

    internal sealed class NoopXmlDecryptor : IXmlDecryptor
    {
        public XElement Decrypt(XElement encryptedElement) => encryptedElement;
    }
}
