using Slugify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Store.BLL.Helper
{
    public class VietnameseSlugHelper : ISlugHelper
    {
        private readonly SlugHelper _slugHelper;

        public VietnameseSlugHelper()
        {
            _slugHelper = new SlugHelper(new SlugHelperConfiguration
            {
                ForceLowerCase = true,
                TrimWhitespace = true
            });
        }

        public SlugHelperConfiguration Config
        {
            get => _slugHelper.Config;
            set => _slugHelper.Config = value;
        }

        public string GenerateSlug(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // 1️⃣ Chuẩn hóa và thay thế ký tự tiếng Việt đặc biệt
            input = NormalizeVietnamese(input);

            // 2️⃣ Dùng Slugify để chuẩn hóa về dạng URL-safe (thay khoảng trắng = "-")
            return _slugHelper.GenerateSlug(input);
        }

        private string NormalizeVietnamese(string text)
        {
            // Thay thế riêng các ký tự “Đ/đ”
            text = text.Replace("Đ", "D").Replace("đ", "d");

            // Chuẩn hóa chuỗi Unicode dạng FormD (tách dấu ra khỏi ký tự)
            string normalized = text.Normalize(NormalizationForm.FormD);

            // Xóa toàn bộ dấu tiếng Việt (ký tự dạng combining)
            Regex regex = new Regex(@"\p{IsCombiningDiacriticalMarks}+");
            string clean = regex.Replace(normalized, string.Empty);

            // Trả về chuỗi đã loại bỏ dấu
            return clean.Normalize(NormalizationForm.FormC);
        }
    }
}
