using System.Collections.Generic;
using Bot.Features.FAQ.Models;

namespace Bot.Features.FAQ
{
    public sealed class Manager
    {
        private readonly Dictionary<int, Faq> faqRegexes = new Dictionary<int, Faq>();

        public void Init(List<Faq> faqs)
        {
            faqs.ForEach((Faq faq) => this.AddFaq(faq));
        }

        public void Reset()
        {
            this.faqRegexes.Clear();
        }

        public void AddFaq(Faq faq)
        {
            this.faqRegexes.Add(faq.Id, faq);
        }

        public void ReplaceFaq(int faqId, Faq faq)
        {
            this.faqRegexes[faqId] = faq;
        }

        public void RemoveFaq(int faqId)
        {
            this.faqRegexes.Remove(faqId);
        }

        public bool TryForAutoResponse(string message, out string response)
        {
            response = string.Empty;

            foreach (Faq faq in this.faqRegexes.Values)
            {
                if (faq.Regex.IsMatch(message))
                {
                    response = faq.Message;
                    return true;
                }
            }

            return false;
        }
    }
}
