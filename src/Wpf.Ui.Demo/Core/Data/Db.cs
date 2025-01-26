// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using DocumentFormat.OpenXml.Bibliography;
using System.Windows.Forms;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.ExtendedProperties;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace LMS.CRM.Core.Data;
public class Users
{
    [Key]
    public int UserID { get; set; }
    [Required]
    [StringLength(50)]
    public string Username { get; set; }
    [Required]
    [StringLength(100)]
    public string PasswordHash { get; set; }
}

public class Members
{
    [Key]
    public int MemberID { get; set; }

    [Required]
    [StringLength(10)]
    public string NationalCode { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public string LastName { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime ExpiryDate { get; set; }

    public byte? MaxBorrowCount { get; set; }
    public DateTime? BirthDate { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string PostCode { get; set; }
    public string FatherName { get; set; }
    public decimal? Debt { get; set; }
    public string Status { get; set; }
}

public class Resources
{
    [Key]
    public int ResourceID { get; set; }

    [Required]
    [StringLength(20)]
    public string ResourceType { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; }

    [Required]
    [StringLength(100)]
    public string Author { get; set; }

    [StringLength(100)]
    public string Publisher { get; set; }

    [StringLength(4)]
    public string PublishYear { get; set; }

    [StringLength(20)]
    public string ISBN { get; set; }

    public short? Quantity { get; set; } = 1;
    public short? AvailableCopies { get; set; } = 1;

    [DataType(DataType.Currency)]
    public decimal? Price { get; set; }
}

public class Reservations
{
    [Key]
    public int ReservationID { get; set; }

    [Required]
    [ForeignKey("Member")]
    public int FK_MemberID { get; set; }

    [Required]
    [ForeignKey("Resource")]
    public int FK_ResourceID { get; set; }

    [Required]
    public DateTime ReservationDate { get; set; }

    [StringLength(10)]
    public string Status { get; set; } = "منتظر";

    public Members Member { get; set; }
    public Resources Resource { get; set; }
}

public class BorrowRecords
{
    [Key]
    public int BorrowID { get; set; }

    [Required]
    [ForeignKey("Member")]
    public int MemberID { get; set; }

    [Required]
    [ForeignKey("Resource")]
    public int ResourceID { get; set; }

    [Required]
    public DateTime BorrowDate { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    [DataType(DataType.Currency)]
    public decimal? Fine { get; set; } = 0;

    public Members Member { get; set; }
    public Resources Resource { get; set; }
}

public class LibraryBranches
{
    [Key]
    public int BranchID { get; set; }

    [Required]
    [StringLength(100)]
    public string BranchName { get; set; }

    [StringLength(200)]
    public string Address { get; set; }

    [StringLength(15)]
    [DataType(DataType.PhoneNumber)]
    public string PhoneNumber { get; set; }

    public byte[] BranchImage { get; set; }
    [NotMapped]
    public ImageSource ImageSource
    {
        get => ByteArrayToImageSource(BranchImage);
        set => BranchImage = ImageSourceToByteArray(value);
    }

    private static ImageSource ByteArrayToImageSource(byte[] byteArray)
    {
        if (byteArray == null || byteArray.Length == 0)
            return null;

        using (var stream = new MemoryStream(byteArray))
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }
    }

    private static byte[] ImageSourceToByteArray(ImageSource imageSource)
    {
        if (imageSource == null)
            return null;

        byte[] bytes = null;
        if (imageSource is BitmapSource bitmapSource)
        {
            var encoder = new JpegBitmapEncoder(); // Use PNG or BMP if needed
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                bytes = stream.ToArray();
            }
        }

        return bytes;
    }
}