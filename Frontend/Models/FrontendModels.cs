﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Frontend.Models
{
    public class MachineModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Mark { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string VIN { get; set; }
        public string StateNumber { get; set; }
        public int IdUsers { get; set; }
    }

    public class UserModel
    {
        public int Id { get; set; }
        public string FIO { get; set; }
        public string Adress { get; set; }
        public string Phone { get; set; }
        public int IdFines { get; set; }
    }

    public class FineModel
    {
        public int Id { get; set; }
        public string NameFine { get; set; }
        public int AmountFine { get; set; }
    }

    public class DetailUserModel
    {
        public string FIO { get; set; }
        public string Adress { get; set; }
        public string Phone { get; set; }
        public string Mark { get; set; }
        public string Model { get; set; }
        public string StateNumber { get; set; }
        public string NameFine { get; set; }
        public int AmountFine { get; set; }
    }

    public class DetailFineModel
    {
        public string FIO { get; set; }
        public string Adress { get; set; }
        public string Phone { get; set; }
        public string NameFine { get; set; }
        public int AmountFine { get; set; }
    }

    public class Authentication
    {
        public string Login { get; set; }
        public string Pass { get; set; }
    }

    public class AuthCodeModel
    {
        public string GrantType { get; set; }
        public string Code { get; set; }
        public string RedirectURI { get; set; }
        public int ClientId { get; set; }
    }

    public class TokenMessage
    {
        public string TokenType { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class RefreshToken
    {
        public string Token { get; set; }
    }
}