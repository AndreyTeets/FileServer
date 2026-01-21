using System.Xml.Linq;
using FileServer.Configuration;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;

namespace FileServer.Tests.Configuration;

internal sealed class InMemoryXmlRepositoryTests
{
#pragma warning disable CS8618 // Non-nullable variable must contain a non-null value when exiting constructor. Consider declaring it as nullable.
    private InMemoryXmlRepository _repo;
#pragma warning restore CS8618 // Remove when `dotnet format` is fixed (see https://github.com/dotnet/sdk/issues/44867)

    [SetUp]
    public void SetUp()
    {
        _repo = new InMemoryXmlRepository();
    }

    [Test]
    public void StoreElement_Then_GetAllElements_ReturnsCopies()
    {
        XElement originalRootElem = new("rootElem", new XElement("childElem", "textValue"));
        _repo.StoreElement(originalRootElem, friendlyName: "not_used");

        IReadOnlyCollection<XElement> elements = _repo.GetAllElements();
        Assert.That(elements, Has.Count.EqualTo(1));
        Assert.That(XNode.DeepEquals(elements.Single(), originalRootElem), Is.True);
        Assert.That(ReferenceEquals(elements.Single(), _repo.GetAllElements().Single()), Is.False);

        elements.Single().Element("childElem")!.Value = "modifiedTextValue";
        Assert.That(_repo.GetAllElements().Single().Element("childElem")!.Value, Is.EqualTo("textValue"));
    }

    [Test]
    public void DeleteElements_RemovesOnlySelectedElements()
    {
        _repo.StoreElement(new XElement("a", "1"), friendlyName: "not_used");
        _repo.StoreElement(new XElement("b", "2"), friendlyName: "not_used");

        _repo.DeleteElements(elements =>
        {
            foreach (IDeletableElement e in elements.Where(e => e.Element.Name == "a"))
                e.DeletionOrder = 0;
        });

        List<XElement> remainingElements = [.. _repo.GetAllElements()];
        Assert.That(remainingElements, Has.Count.EqualTo(1));
        Assert.That(remainingElements.Single().Name.LocalName, Is.EqualTo("b"));
    }

    [Test]
    public void NoopXmlEncryptorDecryptor_BehaveAsNoop()
    {
        InMemoryXmlRepository.NoopXmlEncryptor encryptor = new();
        InMemoryXmlRepository.NoopXmlDecryptor decryptor = new();

        XElement unencryptedElem = new("e");
        EncryptedXmlInfo encryptedElemInfo = encryptor.Encrypt(unencryptedElem);
        XElement encryptedElem = encryptedElemInfo.EncryptedElement;

        Assert.That(encryptedElemInfo.DecryptorType, Is.EqualTo(typeof(InMemoryXmlRepository.NoopXmlDecryptor)));
        Assert.That(ReferenceEquals(encryptedElem, unencryptedElem), Is.True);
        Assert.That(ReferenceEquals(decryptor.Decrypt(encryptedElem), unencryptedElem), Is.True);
        Assert.That(ReferenceEquals(decryptor.Decrypt(unencryptedElem), unencryptedElem), Is.True);
    }
}
