using AddressBookApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Net;
using System.Data.Entity;
using System.Web.Security;
using AddressBookApplication.ViewModel;
using System.Net.Mail;
using System.Security;

using System.Data.Entity.Infrastructure;
using System.Threading;

using WebMatrix.WebData;

namespace AddressBookApplication.Controllers
{
    public class HomeController : Controller
    {
        private AddressBookEntities db = new AddressBookEntities();
        #region Hashing with SHA256
        #region Private Members
        private byte[] _keyByte = { };
        string keyBytes;
        //Default Key
        private static string _key = "";
        //Default initial vector
        private byte[] _ivByte = { 0x01, 0x12, 0x23, 0x34, 0x45, 0x56, 0x67, 0x78 };
        #endregion

        #region Enumarables
        /// <summary> 
        /// Hash enum value 
        /// </summary> 
        public enum HashName
        {
            SHA256 = 1

        }
        #endregion

        #endregion

        [AllowAnonymous]
        // GET: Home
        public ActionResult Login()
        {
            return View();
        }



        [HttpPost]
        public ActionResult Login(string Email, string Password)
        {
            if (ModelState.IsValid)
            {
                LoginCheck_Result result = new LoginCheck_Result();
                keyBytes = ComputeHash(Email, Password, HashName.SHA256);
                string encryptPassword = Encrypt(Password, keyBytes, string.Empty);


                result = db.LoginCheck(Email, encryptPassword).SingleOrDefault();
                // ViewData["data"] = result.FirstName;

                if (result == null)
                {
                    ModelState.AddModelError("", "Invaild Email Or Password");
                }
                else
                {
                    if (result.Email == Email)
                    {
                        Session["id"] = result.UserId;

                        if (result.UserInfo == null)
                        {

                            return RedirectToAction("Main", "NormalUserContacts");
                        }
                        else if (result.UserInfo.Trim() == "Admin")
                        {

                            return RedirectToAction("Main", "Admin");

                        }
                    }
                }

            }
            return View();

            //
            //    return RedirectToAction("Login");


            //  Response.Write(comp);
            //  return View(userDetail);
        }


        #region methods

        /// <summary> 
        /// Encrypt text by key with initialization vector 
        /// </summary> 
        /// <param name="value">plain text</param> 
        /// <param name="key"> string key</param> 
        /// <param name="iv">initialization vector</param> 
        /// <returns>encrypted text</returns> 
        public string Encrypt(string value, string key, string iv)
        {
            string encryptValue = string.Empty;
            MemoryStream ms = null;
            CryptoStream cs = null;
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        _keyByte = Encoding.UTF8.GetBytes
                                (key.Substring(0, 8));
                        if (!string.IsNullOrEmpty(iv))
                        {
                            _ivByte = Encoding.UTF8.GetBytes
                                (iv.Substring(0, 8));
                        }
                    }
                    else
                    {
                        _keyByte = Encoding.UTF8.GetBytes(_key);
                    }
                    using (DESCryptoServiceProvider des =
                            new DESCryptoServiceProvider())
                    {

                        byte[] inputByteArray =
                            Encoding.UTF8.GetBytes(value);
                        ms = new MemoryStream();
                        cs = new CryptoStream(ms, des.CreateEncryptor
                        (_keyByte, _ivByte), CryptoStreamMode.Write);
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        encryptValue = Convert.ToBase64String(ms.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    //TODO: write log 
                    Response.Write(ex.Message.ToString());
                }
                finally
                {
                    cs.Dispose();
                    ms.Dispose();
                }
            }
            // string x = Decrypt(encryptValue, keyBytes, string.Empty);
            return encryptValue;

        }
        public string Decrypt(string value, string key, string iv)
        {
            string decrptValue = string.Empty;
            if (!string.IsNullOrEmpty(value))
            {
                MemoryStream ms = null;
                CryptoStream cs = null;
                value = value.Replace(" ", "+");
                byte[] inputByteArray = new byte[value.Length];
                try
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        _keyByte = Encoding.UTF8.GetBytes
                                (key.Substring(0, 8));
                        if (!string.IsNullOrEmpty(iv))
                        {
                            _ivByte = Encoding.UTF8.GetBytes
                                (iv.Substring(0, 8));
                        }
                    }
                    else
                    {
                        _keyByte = Encoding.UTF8.GetBytes(_key);
                    }
                    using (DESCryptoServiceProvider des =
                            new DESCryptoServiceProvider())
                    {
                        inputByteArray = Convert.FromBase64String(value);
                        ms = new MemoryStream();
                        cs = new CryptoStream(ms, des.CreateDecryptor
                        (_keyByte, _ivByte), CryptoStreamMode.Write);
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        Encoding encoding = Encoding.UTF8;
                        decrptValue = encoding.GetString(ms.ToArray());
                    }
                }
                catch
                {
                    //TODO: write log 
                }
                finally
                {
                    cs.Dispose();
                    ms.Dispose();
                }
            }
            return decrptValue;
        }
        /// <summary> 
        /// Compute Hash 
        /// </summary> 
        /// <param name="plainText">plain text</param> 
        /// <param name="salt">salt string</param> 
        /// <returns>string</returns> 
        public string ComputeHash(string plainText, string salt)
        {
            return ComputeHash(plainText, salt, HashName.SHA256);
        }

        /// <summary> 
        /// Compute Hash 
        /// </summary> 
        /// <param name="plainText">plain text</param> 
        /// <param name="salt">salt string</param> m
        /// <param name="hashName">Hash Name</param> 
        /// <returns>string</returns> 
        public string ComputeHash(string plainText, string salt, HashName hashName)
        {
            if (!string.IsNullOrEmpty(plainText))
            {
                // Convert plain text into a byte array. 
                byte[] plainTextBytes = ASCIIEncoding.ASCII.GetBytes(plainText);
                // Allocate array, which will hold plain text and salt. 
                byte[] plainTextWithSaltBytes = null;
                byte[] saltBytes;
                if (!string.IsNullOrEmpty(salt))
                {
                    // Convert salt text into a byte array. 
                    saltBytes = ASCIIEncoding.ASCII.GetBytes(salt);
                    plainTextWithSaltBytes =
                        new byte[plainTextBytes.Length + saltBytes.Length];
                }
                else
                {
                    // Define min and max salt sizes. 
                    int minSaltSize = 4;
                    int maxSaltSize = 8;
                    // Generate a random number for the size of the salt. 
                    Random random = new Random();
                    int saltSize = random.Next(minSaltSize, maxSaltSize);
                    // Allocate a byte array, which will hold the salt. 
                    saltBytes = new byte[saltSize];
                    // Initialize a random number generator. 
                    RNGCryptoServiceProvider rngCryptoServiceProvider =
                                new RNGCryptoServiceProvider();
                    // Fill the salt with cryptographically strong byte values. 
                    rngCryptoServiceProvider.GetNonZeroBytes(saltBytes);
                }
                // Copy plain text bytes into resulting array. 
                for (int i = 0; i < plainTextBytes.Length; i++)
                {
                    plainTextWithSaltBytes[i] = plainTextBytes[i];
                }
                // Append salt bytes to the resulting array. 
                for (int i = 0; i < saltBytes.Length; i++)
                {
                    plainTextWithSaltBytes[plainTextBytes.Length + i] =
                                        saltBytes[i];
                }
                HashAlgorithm hash = null;
                if (hashName == HashName.SHA256)
                {

                    hash = new SHA256Managed();


                }
                // Compute hash value of our plain text with appended salt. 
                byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);
                // Create array which will hold hash and original salt bytes. 
                byte[] hashWithSaltBytes =
                    new byte[hashBytes.Length + saltBytes.Length];
                // Copy hash bytes into resulting array. 
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    hashWithSaltBytes[i] = hashBytes[i];
                }
                // Append salt bytes to the result. 
                for (int i = 0; i < saltBytes.Length; i++)
                {
                    hashWithSaltBytes[hashBytes.Length + i] = saltBytes[i];
                }
                // Convert result into a base64-encoded string. 
                string hashValue = Convert.ToBase64String(hashWithSaltBytes);
                // Return the result. 
                return hashValue;
            }
            return string.Empty;
        }
        #endregion
        //TO update profile when loged in as Normal User
        public ActionResult MyProfile()
        {
            long? id = Convert.ToInt64(Session["id"]);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserDetail userDetail = db.UserDetails.Find(id);
            if (userDetail == null)
            {
                return HttpNotFound();
            }
            return View(userDetail);
            // return RedirectToAction("Login");
        }


        public ActionResult UpdatePassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UpdatePassword(string Password, string confirmPassword)
        {
            long id = Convert.ToInt64(Session["id"]);
            UserDetail userDetail = db.UserDetails.Find(id);
            if (Password == confirmPassword)
            {
                keyBytes = ComputeHash(userDetail.Email, Password, HashName.SHA256);

                string encryptPassword = Encrypt(Password, keyBytes, string.Empty);
                userDetail.Password = encryptPassword;

                db.Entry(userDetail).State = EntityState.Modified;
                db.SaveChanges();

            }
            else
            {
                ModelState.AddModelError("", "New Password and Confirm Password are not same");
            }
            return View();
        }
        // Save changes when User click save button to edit his/her profile details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MyProfile([Bind(Include = "UserId,FirstName,LastName,Email,Password")] UserDetail userDetail)
        {
            if (ModelState.IsValid)
            {
                //db.Entry(userDetail).State = EntityState.Modified;
                //db.SaveChanges();
                var obj = db.UserDetails.Find(userDetail.UserId);
                obj.FirstName = userDetail.FirstName;
                obj.LastName = userDetail.LastName;
                obj.Email = userDetail.Email;
                db.SaveChanges();

                // return RedirectToAction("Index");
                return RedirectToAction("Main", "NormalUserContacts");
            }
            else
            {
                return View(userDetail);
            }
        }


    }
}