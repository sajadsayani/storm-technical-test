using DatabaseAccess;
using System.Data;
using System.Xml.Linq;

namespace CXmlInvoiceGenerator
{
    internal class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("New invoice generation run starting at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            GenerateCXMLForNewInvoices();
            Console.WriteLine("New invoice generation run finishing at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }


        private static void GenerateCXMLForNewInvoices()
        {

            // == Please complete this function ==

            // 1) Using the DatabaseAccess dll provided and referenced (in the refs folder), load each invoice from the database
            //
            // 2) Create a cXml invoice document using the information from each invoice

            // The following is a very helpful resource for cXml:

            // https://compass.coupa.com/en-us/products/product-documentation/supplier-resources/supplier-integration-resources/standard-invoice-examples/sample-cxml-invoice-with-payment-terms

            // Assume the invoice is raised on the same day you find it, so PaymentTerms is from Today

            // VAT mode is header (overall total) only, not at item level

            // 3) Save the created invoices into a specified output file with the .xml file extension

            // The "purpose" for each invoice is "standard"
            // The "operation" for each invoice is "new"
            // The output folder is entirely up to you, based on your file system
            // You can use "fake" credentials (Domain/Identity/SharedSecret etc. etc.) of your own choosing for the From/To/Sender section for this test
            //
            // It would likely be a good idea for all of these to be configurable in some way, in a .Net options/settings file or an external ini file

            // Ideally, you will write reasonable progress steps to the console window

            // You may add references to anything you want from the standard Nuget URL

            // You may modify the signature to this function if you want to pass values into it

            // You may move this code into another class (or indeed classes) of your choosing

            Invoices invoiceDB = new();
            DataTable newInvoices = invoiceDB.GetNewInvoices();

            // invoiceDB contains other functions you will need...


            // Now we will add the child Element  
            Console.WriteLine("Reading each invoice from the database");
            foreach (DataRow row in newInvoices.Rows)
            {
                Console.WriteLine("Processing Invoice: " + row["Id"].ToString());

                // creating the Document  
                var invoiceXml = new XDocument();

                //Now, we are Adding the Root Element  
                Console.WriteLine("Creating Root Element for XML file");
                var rootElement = new XElement(
                    "cXML", 
                    new XAttribute("version", "1"), 
                    new XAttribute("timestamp", DateTime.Now)
                );
                invoiceXml.Add(rootElement);

                //Now, we are Adding the Header Element  
                Console.WriteLine("Creating Header Elements for XML file");
                var headerRootElement = new XElement("Header");

                rootElement.Add(headerRootElement);

                // Setup the children for Header
                var fromElement = new XElement("From");
                var toElement = new XElement("To");
                var senderElement = new XElement("Sender");
                var fromCredentialElement = new XElement("Credential", new XAttribute("domain", "DUNS"));
                var toCredentialElement = new XElement("Credential", new XAttribute("domain", "NetworkID"));
                var senderCredentialElement = new XElement("Credential", new XAttribute("domain", "DUNS"));
                var identityElement = new XElement("Identity", "xxxxxxxxxxxx");
                var sharedSecretElement = new XElement("SharedSecret", "xxxxxxxxxxxx");
                var vatModeElement = new XElement("VATMode");
                var vatCodeElement = new XElement("VATCode", row["VATCode"].ToString());
                var vatPercentageElement = new XElement("VATPercentage", row["VATPercentage"].ToString());
                var vatAmountElement = new XElement("VATAmount", row["VATAmount"].ToString());

                // Add the children for Header
                headerRootElement.Add(fromElement);
                headerRootElement.Add(toElement);
                headerRootElement.Add(senderElement);
                headerRootElement.Add(vatModeElement);
                fromElement.Add(fromCredentialElement);
                fromCredentialElement.Add(identityElement);
                toElement.Add(toCredentialElement);
                toCredentialElement.Add(identityElement);
                senderElement.Add(senderCredentialElement);
                senderCredentialElement.Add(identityElement);
                senderCredentialElement.Add(sharedSecretElement);
                vatModeElement.Add(vatCodeElement);
                vatModeElement.Add(vatPercentageElement);
                vatModeElement.Add(vatAmountElement);

                //Now, we are Adding the Detail Request Elements  
                Console.WriteLine("Creating Detail Request Elements for XML file");
                var requestRootElement = new XElement(
                    "Request",
                    new XAttribute("deploymentMode", "Test")
                );
                rootElement.Add(requestRootElement);

                // Setup the children for Request
                var invoiceDetailRequestElement = new XElement("InvoiceDetailRequest");

                var invoiceDetailRequestHeaderElement = new XElement(
                    "InvoiceDetailRequestHeader",
                    new XAttribute("invoiceId", row["Id"].ToString()),
                    new XAttribute("purpose", "Standard"),
                    new XAttribute("operation", "New"),
                    new XAttribute("invoiceDate", DateTime.Now)
                );

                // Fetch Delivery address
                Console.WriteLine("Fetching Delivery Address Details from DB");
                DataRow deliveryAdress = invoiceDB.GetDeliveryAddressForSalesOrder(int.Parse(row["SalesOrderId"].ToString()));

                var invoiceDeliveryPartnerElement = new XElement("InvoicePartner");
                var contactDeliveryAddressElement = new XElement("Contact", new XAttribute("role", "deliveryAddress"));
                var nameDeliveryAddressElement = new XElement("Name", new XAttribute("xml-lang", "en-UK"), deliveryAdress["ContactName"].ToString());
                var postalDeliveryAddressElement = new XElement("PostalAddress");
                var postalDeliveryAddressLine1Element = new XElement("HouseNameStreet", deliveryAdress["AddrLine1"].ToString());
                var postalDeliveryAddressLine2Element = new XElement("Street", deliveryAdress["AddrLine2"].ToString());
                var postalDeliveryAddressLine3Element = new XElement("Town", deliveryAdress["AddrLine3"].ToString());
                var postalDeliveryAddressLine4Element = new XElement("County", deliveryAdress["AddrLine4"].ToString());
                var postalDeliveryAddressLine5Element = new XElement("AddressLine5", deliveryAdress["AddrLine5"].ToString());
                var postalDeliveryPostcodeElement = new XElement("Postcode", deliveryAdress["AddrPostCode"].ToString());
                var postalDeliveryCountryElement = new XElement("Country", new XAttribute("isoCountryCode", deliveryAdress["CountryCode"].ToString()), deliveryAdress["Country"].ToString());

                // Fetch Billing address
                Console.WriteLine("Fetching Billing Address Details from DB");
                DataRow billingAdress = invoiceDB.GetBillingAddressForInvoice(int.Parse(row["Id"].ToString()));

                var invoiceBillingPartnerElement = new XElement("InvoicePartner");
                var contactBillingAddressElement = new XElement("Contact", new XAttribute("role", "billingAddress"));
                var nameBillingAddressElement = new XElement("Name", new XAttribute("xml-lang", "en-UK"), billingAdress["ContactName"].ToString());
                var postalBillingAddressElement = new XElement("PostalAddress");
                var postalBillingAddressLine1Element = new XElement("HouseNameStreet", billingAdress["AddrLine1"].ToString());
                var postalBillingAddressLine2Element = new XElement("Street", billingAdress["AddrLine2"].ToString());
                var postalBillingAddressLine3Element = new XElement("Town", billingAdress["AddrLine3"].ToString());
                var postalBillingAddressLine4Element = new XElement("County", billingAdress["AddrLine4"].ToString());
                var postalBillingAddressLine5Element = new XElement("AddressLine5", billingAdress["AddrLine5"].ToString());
                var postalBillingPostcodeElement = new XElement("Postcode", billingAdress["AddrPostCode"].ToString());
                var postalBillingCountryElement = new XElement("Country", new XAttribute("isoCountryCode", billingAdress["CountryCode"].ToString()), billingAdress["Country"].ToString());

                // Calculate Payment Date
                DateTime d1 = DateTime.Now;
                TimeSpan span = new TimeSpan(int.Parse(row["PaymentTermsDays"].ToString()), 0, 0, 0);
                DateTime d2 = d1.Add(span);

                var paymentTermElement = new XElement("PaymentTerm", new XAttribute("payInNumberofDays", row["PaymentTermsDays"].ToString()));
                var paymentTermDueDateElement = new XElement("PaymentDueDate", d2);


                // Add the children for Request
                requestRootElement.Add(invoiceDetailRequestElement);

                invoiceDetailRequestElement.Add(invoiceDetailRequestHeaderElement);
                
                invoiceDetailRequestHeaderElement.Add(invoiceDeliveryPartnerElement);
                invoiceDeliveryPartnerElement.Add(contactDeliveryAddressElement);
                contactDeliveryAddressElement.Add(nameDeliveryAddressElement);
                contactDeliveryAddressElement.Add(postalDeliveryAddressElement);
                postalDeliveryAddressElement.Add(postalDeliveryAddressLine1Element);
                postalDeliveryAddressElement.Add(postalDeliveryAddressLine2Element);
                postalDeliveryAddressElement.Add(postalDeliveryAddressLine3Element);
                postalDeliveryAddressElement.Add(postalDeliveryAddressLine4Element);
                postalDeliveryAddressElement.Add(postalDeliveryAddressLine5Element);
                postalDeliveryAddressElement.Add(postalDeliveryPostcodeElement);
                postalDeliveryAddressElement.Add(postalDeliveryCountryElement);

                invoiceDetailRequestHeaderElement.Add(invoiceBillingPartnerElement);
                invoiceBillingPartnerElement.Add(contactBillingAddressElement);
                contactBillingAddressElement.Add(nameBillingAddressElement);
                contactBillingAddressElement.Add(postalBillingAddressElement);
                postalBillingAddressElement.Add(postalBillingAddressLine1Element);
                postalBillingAddressElement.Add(postalBillingAddressLine2Element);
                postalBillingAddressElement.Add(postalBillingAddressLine3Element);
                postalBillingAddressElement.Add(postalBillingAddressLine4Element);
                postalBillingAddressElement.Add(postalBillingAddressLine5Element);
                postalBillingAddressElement.Add(postalBillingPostcodeElement);
                postalBillingAddressElement.Add(postalBillingCountryElement);

                invoiceDetailRequestHeaderElement.Add(paymentTermElement);
                paymentTermElement.Add(paymentTermDueDateElement);


                // Now, lets walk through the items on the invoice and output these
                Console.WriteLine("Fetching Invoice Items from DB");
                DataTable invoiceItems = invoiceDB.GetItemsOnInvoice(int.Parse(row["Id"].ToString()));

                var count = 0;
                foreach (DataRow invoiceItem in invoiceItems.Rows)
                {
                    count++;

                    // Setup the elements for Invoice Details
                    var invoiceDetailOrderElement = new XElement("InvoiceDetailOrder");
                    var invoiceDetailOrderInfoElement = new XElement("InvoiceDetailOrderInfo");
                    var orderReferenceElement = new XElement("OrderReference");
                    var documentReferenceElement = new XElement("DocumentReference", new XAttribute("payloadID", invoiceItem["Id"].ToString()));
                    var invoiceDetailItemElement = new XElement("InvoiceDetailItem", new XAttribute("invoiceLineNumber", count.ToString()), new XAttribute("quantity", invoiceItem["Qty"].ToString()));
                    var invoiceDetailItemReferenceElement = new XElement("InvoiceDetailItemReference");
                    var invoiceItemIDElement = new XElement("ItemID");
                    var invoiceStockItemIdElement = new XElement("StockItemId", invoiceItem["StockItemId"].ToString());
                    var invoiceItemDescriptionElement = new XElement("Description", new XAttribute("xml-lang", "en-UK"), invoiceItem["Description"].ToString());
                    var invoiceManufacturerPartIDElement = new XElement("ManufacturerPartID", invoiceItem["PartNo"].ToString());
                    var invoiceManufacturerNameElement = new XElement("ManufacturerName", new XAttribute("xml-lang", "en-UK"), invoiceItem["Manufacturer"].ToString());
                    var itemUnitPriceElement = new XElement("UnitPrice");
                    var itemUnitPriceMoneyElement = new XElement("Money", new XAttribute("currency", "GBP"), invoiceItem["UnitPrice"].ToString());
                    var itemLineTotalElement = new XElement("LineTotal");
                    var itemLineTotalMoneyElement = new XElement("Money", new XAttribute("currency", "GBP"), invoiceItem["LineTotal"].ToString());
                    var itemLineNetAmountElement = new XElement("NetAmount");
                    var itemLineNetAmountMoneyElement = new XElement("Money", new XAttribute("currency", "GBP"), invoiceItem["LineTotal"].ToString());

                    // Add the children for Request
                    invoiceDetailRequestElement.Add(invoiceDetailOrderElement);
                    invoiceDetailOrderElement.Add(invoiceDetailOrderInfoElement);
                    invoiceDetailOrderInfoElement.Add(orderReferenceElement);
                    orderReferenceElement.Add(documentReferenceElement);

                    invoiceDetailOrderElement.Add(invoiceDetailItemElement);

                    invoiceDetailItemElement.Add(invoiceDetailItemReferenceElement);
                    invoiceDetailItemReferenceElement.Add(invoiceItemIDElement);
                    invoiceItemIDElement.Add(invoiceStockItemIdElement);

                    invoiceDetailItemReferenceElement.Add(invoiceItemDescriptionElement);
                    invoiceDetailItemReferenceElement.Add(invoiceManufacturerPartIDElement);
                    invoiceDetailItemReferenceElement.Add(invoiceManufacturerNameElement);

                    invoiceDetailItemElement.Add(itemUnitPriceElement);
                    itemUnitPriceElement.Add(itemUnitPriceMoneyElement);

                    invoiceDetailItemElement.Add(itemLineTotalElement);
                    itemLineTotalElement.Add(itemLineTotalMoneyElement);

                    invoiceDetailItemElement.Add(itemLineNetAmountElement);
                    itemLineNetAmountElement.Add(itemLineNetAmountMoneyElement);
                }


                // Now, we will save the Created File  
                Console.WriteLine("Saving File in the following directory: C:\\Users\\Kookie\\Downloads\\");
                invoiceXml.Save("C:\\Users\\Kookie\\Downloads\\Invoice_" + row["Id"].ToString() + ".xml");
                //Console.WriteLine(invoiceXml.ToString());
            }

            Console.ReadKey();

        }




    }
}