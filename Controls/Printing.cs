using System;
using System.Collections.Generic;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Xps;

namespace ImagingSIMS.Controls
{
    public class Printing
    {
        /// <summary>
        /// Returns a PrintTicket based on the current default printer.
        /// </summary>
        /// <returns>A PrintTicket for the current local default printer.</returns>
        public static PrintTicket GetPrintTicketFromPrinter()
        {
            PrintQueue printQueue = null;

            LocalPrintServer localPrintServer = new LocalPrintServer();

            PrintQueueCollection localPrinterCollection = localPrintServer.GetPrintQueues();

            System.Collections.IEnumerator localPrinterEnumerator = localPrinterCollection.GetEnumerator();

            if (localPrinterEnumerator.MoveNext())
            {
                printQueue = (PrintQueue)localPrinterEnumerator.Current;
            }
            else return null;

            PrintTicket printTicket = printQueue.DefaultPrintTicket;

            PrintCapabilities printCapabilities = printQueue.GetPrintCapabilities();

            if (printCapabilities.CollationCapability.Contains(Collation.Collated))
            {
                printTicket.Collation = Collation.Collated;
            }
            if (printCapabilities.DuplexingCapability.Contains(Duplexing.TwoSidedLongEdge))
            {
                printTicket.Duplexing = Duplexing.TwoSidedLongEdge;
            }
            if (printCapabilities.StaplingCapability.Contains(Stapling.StapleDualLeft))
            {
                printTicket.Stapling = Stapling.StapleDualLeft;
            }

            return printTicket;
        }
   
        /// <summary>
        /// Returns an XpsDocumentWriter for the default print queue.
        /// </summary>
        /// <returns>An XpsDocumentWriter for the default print queue.</returns>
        public static XpsDocumentWriter GetPrintXpsDocumentWriter()
        {
            LocalPrintServer ps = new LocalPrintServer();

            PrintQueue pq = ps.DefaultPrintQueue;

            XpsDocumentWriter xpsdw = PrintQueue.CreateXpsDocumentWriter(pq);

            return xpsdw;
        }
    }
}
