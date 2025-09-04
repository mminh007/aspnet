using Backend.User.Databases;
using Backend.User.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Backend.User.Enums;

namespace Backend.User.Repository
{
    public class UserRepository : IUserRepository
    {

        public readonly UserDbContext _db;
        //private readonly PasswordHasher<UserModel> _passwordHasher = new PasswordHasher<UserModel>();

        public UserRepository(UserDbContext db)
        {
            _db = db;
        }


        public async Task<UserResponseModel> CreateUserAsync(UserCheckModel user)
        {
            try
            {
                // Check email tồn tại
                var existed = await _db.Users
                                       .FirstOrDefaultAsync(u => u.Email == user.Email);

                if (existed != null)
                {
                    return new UserResponseModel
                    {
                        Success = false,
                        Message = OperationResult.Failed,
                        ErrorMessage = "Email already exists"
                    };
                }

                // Tạo user mới
                var newUser = new UserModel
                {
                    UserId = Guid.NewGuid(),
                    Email = user.Email,
                    // Password = _passwordHasher.HashPassword(newUser, user.Password)
                    CreatedAt = DateTime.Now,
                };

                _db.Users.Add(newUser);
                await _db.SaveChangesAsync();

                return new UserResponseModel
                {
                    UserId = newUser.UserId,
                    Success = true,
                    Message = OperationResult.Success
                };
            }
            catch (Exception ex)
            {
                // Log error
                //_logger.LogError(ex, "Error creating user");

                return new UserResponseModel
                {
                    Success = false,
                    Message = OperationResult.Error, // bạn có thể thêm enum OperationResult.Error
                    ErrorMessage = ex.Message
                };
            }
        }


        public async Task<UserResponseModel> DeleteUserAsync(Guid userId)
        {
            try
            {
                var userCheck = await _db.Users
                                         .FirstOrDefaultAsync(u => u.UserId == userId);

                if (userCheck == null)
                {
                    return new UserResponseModel
                    {
                        Success = false,
                        Message = OperationResult.NotFound,
                        ErrorMessage = "User not found"
                    };
                }

                _db.Users.Remove(userCheck);
                await _db.SaveChangesAsync();

                return new UserResponseModel
                {
                    UserId = userId,
                    Success = true,
                    Message = OperationResult.Success
                };
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error deleting user {UserId}", userId);

                return new UserResponseModel
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = ex.Message
                };
            }
        }


        public async Task<UserResponseModel> UpdateUserAsync(UpdateUser model)
        {
            try
            {
                var userCheck = await _db.Users
                                         .FirstOrDefaultAsync(u => u.UserId == model.UserId);

                if (userCheck == null)
                {
                    return new UserResponseModel
                    {
                        Success = false,
                        Message = OperationResult.NotFound,
                        ErrorMessage = "User not found"
                    };
                }

                userCheck.PhoneNumber = model.PhoneNumber;
                userCheck.Address = model.Address;

                _db.Users.Update(userCheck);
                await _db.SaveChangesAsync();

                return new UserResponseModel
                {
                    UserId = userCheck.UserId,
                    Success = true,
                    Message = OperationResult.Success
                };
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error updating user {UserId}", model.UserId);

                return new UserResponseModel
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = ex.Message
                };
            }
        }


        //public async Task<UserResponseModel> ValidateUserAsync(LoginModel user)
        //{
        //    var existingUser = await _db.Users
        //                        .FirstOrDefaultAsync(u => u.Email == user.Email);

        //    if (existingUser == null)
        //    {
        //        return new UserResponseModel { Success = false, Message = "Email not exists" };
        //    }

        //    var result = _passwordHasher.VerifyHashedPassword(existingUser, existingUser.Password, user.Password);

        //    if (result == PasswordVerificationResult.Success) 
        //    {
        //        return new UserResponseModel { Success = true,
        //                                       Message = "Login Success!",
        //                                       UserId = existingUser.UserId
        //                                      };
        //    }
        //    else
        //    {
        //        return new UserResponseModel {
        //                                        Success = false,
        //                                        Message = "Invalid password!"
        //                                     };

        //    }


        //}
    }


}
