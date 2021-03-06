using System;
using Crud.Net.Core.DataContract;
using Crud.Net.Core.DataContractMapper;
using Crud.Net.Core.DataModel;
using Crud.Net.Core.Repository;
using Crud.Net.Core.Service;
using Crud.Net.Core.UnitOfWork;
using Moq;
using NUnit.Framework;

namespace Crud.Net.Core.Tests.Service
{
    public class TestEntity : IEntity
    {
        public long Id { get; set; }
        public DateTime CreatedUTC { get; set; }
        public DateTime ModifiedUTC { get; set; }
    }

    public class TestDto : ICrudDto {
        public long Id { get; set; }
        public DateTime CreatedUTC { get; set; }
        public DateTime ModifiedUTC { get; set; }
    }

    public class TestContext {

    }
    
    [TestFixture]
    public class CrudServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<ICrudRepository<TestEntity>> _mockRepository;
        private Mock<ICrudDtoMapper<TestEntity, TestDto>> _mockDataContractMapper;
        
        [SetUp]
        public void Init(){
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockRepository = new Mock<ICrudRepository<TestEntity>>();
            _mockDataContractMapper = new Mock<ICrudDtoMapper<TestEntity, TestDto>>();
        }

        [Test]
        public void Test_NoUnitofWork_ArgumentNullException(){
            Assert.Throws<ArgumentNullException>(() => {
                var manager = new CrudService<TestContext, TestEntity, TestDto>(null, _mockRepository.Object, _mockDataContractMapper.Object);
            });
        }

        [Test]
        public void Test_NoRepository_ArgumentNullException(){
            Assert.Throws<ArgumentNullException>(() => {
                var manager = new CrudService<TestContext, TestEntity, TestDto>(_mockUnitOfWork.Object, null, _mockDataContractMapper.Object);
            });
        }

        [Test]
        public void Test_NoDtoMapper_ArgumentNullException(){
            Assert.Throws<ArgumentNullException>(() => {
                var manager = new CrudService<TestContext, TestEntity, TestDto>(_mockUnitOfWork.Object, _mockRepository.Object, null);
            });
        }

        [Test]
        public void Test_GetById_EntityNotFound_ReturnNull(){
            _mockRepository.Setup(m => m.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(null);

            var manager = new CrudService<TestContext, TestEntity, TestDto>(_mockUnitOfWork.Object, _mockRepository.Object, _mockDataContractMapper.Object);

            var result = manager.GetByIdAsync(1);

            Assert.IsNull(result.Result);
        }

        [Test]
        public void Test_GetById_EntityFound_Mapped(){
            var testEntity = new TestEntity();
            var testDto = new TestDto();

            _mockRepository.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(testEntity);

            _mockDataContractMapper.Setup(m => m.MapEntityToDto(testEntity)).Returns(testDto);

            var manager = new CrudService<TestContext, TestEntity, TestDto>(_mockUnitOfWork.Object, _mockRepository.Object, _mockDataContractMapper.Object);

            var result = manager.GetByIdAsync(1);

            Assert.IsNotNull(result.Result);
            Assert.IsTrue(result.Result == testDto);
            _mockDataContractMapper.Verify(m => m.MapEntityToDto(testEntity), Times.Once);
        }

        [Test]
        public void Test_Add_NullDto_ArgumentNullException(){
            var manager = new CrudService<TestContext, TestEntity, TestDto>(_mockUnitOfWork.Object, _mockRepository.Object, _mockDataContractMapper.Object);

            Assert.Throws<AggregateException>(() => {
                var res = manager.AddAsync(null).Result;
            });
        }

        [Test]
        public void Test_Add_DtoIdForcedToBeZero(){
            var testEntity = new TestEntity();
            var testDto = new TestDto();
            testDto.Id = 5;

            _mockDataContractMapper.Setup(m => m.MapDtoToEntity(It.IsAny<TestDto>())).Returns(testEntity);

            _mockDataContractMapper.Setup(m => m.MapEntityToDto(testEntity)).Returns(testDto);

            var manager = new CrudService<TestContext, TestEntity, TestDto>(_mockUnitOfWork.Object, _mockRepository.Object, _mockDataContractMapper.Object);

            var result = manager.AddAsync(testDto).Result;

            Assert.IsTrue(testDto.Id == 0);
        }
        
        [Test]
        public void Test_Add_EntityMapped_DtoReturned(){
            var testEntity = new TestEntity();
            var testDto = new TestDto();

            _mockDataContractMapper.Setup(m => m.MapDtoToEntity(It.IsAny<TestDto>())).Returns(testEntity);

            _mockDataContractMapper.Setup(m => m.MapEntityToDto(testEntity)).Returns(testDto);

            var manager = new CrudService<TestContext, TestEntity, TestDto>(_mockUnitOfWork.Object, _mockRepository.Object, _mockDataContractMapper.Object);

            var result = manager.AddAsync(new TestDto()).Result;

            Assert.IsTrue(result == testDto);
            _mockRepository.Verify(m => m.Add(testEntity), Times.Once);
            _mockUnitOfWork.Verify(m => m.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void Test_Add_Created_And_Modified_Dates_Provided(){
            var testEntity = new TestEntity();
            var testDto = new TestDto();

            _mockDataContractMapper.Setup(m => m.MapDtoToEntity(It.IsAny<TestDto>())).Returns(testEntity);

            _mockDataContractMapper.Setup(m => m.MapEntityToDto(testEntity)).Returns(testDto);

            var manager = new CrudService<TestContext, TestEntity, TestDto>(_mockUnitOfWork.Object, _mockRepository.Object, _mockDataContractMapper.Object);

            var result = manager.AddAsync(testDto).Result;

            Assert.IsTrue(testDto.CreatedUTC.Year == DateTime.UtcNow.Year && testDto.CreatedUTC.Month == DateTime.UtcNow.Month && testDto.CreatedUTC.Day == DateTime.UtcNow.Day);

            Assert.IsTrue(testDto.ModifiedUTC.Year == DateTime.UtcNow.Year && testDto.ModifiedUTC.Month == DateTime.UtcNow.Month && testDto.ModifiedUTC.Day == DateTime.UtcNow.Day);
        }

        [Test]
        public void Test_Update_NullDto_ArgumentNullException(){
            var manager = new CrudService<TestContext, TestEntity, TestDto>(_mockUnitOfWork.Object, _mockRepository.Object, _mockDataContractMapper.Object);

            Assert.Throws<AggregateException>(() => {
                var res = manager.UpdateAsync(null).Result;
            });
        }

        [Test]
        public void Test_Update_EntityNotFound_NullReturned(){
            var testEntity = new TestEntity();
            var testDto = new TestDto();
            testDto.Id = 2;

            _mockRepository.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(testEntity);

            var manager = new CrudService<TestContext, TestEntity, TestDto>(_mockUnitOfWork.Object, _mockRepository.Object, _mockDataContractMapper.Object);

            var result = manager.UpdateAsync(testDto).Result;

            Assert.IsNull(result);
        }

        [Test]
        public void Test_Update_EntityMapped_DtoReturned(){
            var testEntity = new TestEntity();
            var testDto = new TestDto();
            testDto.Id = 1;

            _mockRepository.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(testEntity);
            
            _mockDataContractMapper.Setup(m => m.MapDtoToEntity(testDto, testEntity)).Returns(testEntity);

            _mockDataContractMapper.Setup(m => m.MapEntityToDto(testEntity)).Returns(testDto);

            var manager = new CrudService<TestContext, TestEntity, TestDto>(_mockUnitOfWork.Object, _mockRepository.Object, _mockDataContractMapper.Object);

            var result = manager.UpdateAsync(testDto).Result;

            Assert.IsTrue(result == testDto);
            _mockRepository.Verify(m => m.Update(testEntity), Times.Once);
            _mockUnitOfWork.Verify(m => m.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void Test_Update_Modified_Date_Generated_Created_Date_Not_Updated(){
            var testEntity = new TestEntity();
            testEntity.CreatedUTC = new DateTime(1997,12,12);
            testEntity.ModifiedUTC = new DateTime(1997,12,13);
            testEntity.Id = 1;
            
            var testDto = new TestDto();
            testDto.Id = 1;
            testDto.CreatedUTC = new DateTime(1999,12,12);
            testDto.ModifiedUTC = new DateTime(1999,12,13);

            _mockRepository.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(testEntity);
            
            _mockDataContractMapper.Setup(m => m.MapDtoToEntity(testDto, testEntity)).Returns(testEntity);

            _mockDataContractMapper.Setup(m => m.MapEntityToDto(testEntity)).Returns(testDto);

            var manager = new CrudService<TestContext, TestEntity, TestDto>(_mockUnitOfWork.Object, _mockRepository.Object, _mockDataContractMapper.Object);

            var result = manager.UpdateAsync(testDto).Result;

            Assert.IsTrue(testDto.CreatedUTC.Year == 1997 && testDto.CreatedUTC.Month == 12 && testDto.CreatedUTC.Day == 12);

            Assert.IsTrue(testDto.ModifiedUTC.Year == DateTime.UtcNow.Year && testDto.ModifiedUTC.Month == DateTime.UtcNow.Month && testDto.ModifiedUTC.Day == DateTime.UtcNow.Day);
        }

        [Test]
        public void Test_Delete_EntityNotFound_ReturnFalse(){
            _mockRepository.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(null);
            
            var manager = new CrudService<TestContext, TestEntity, TestDto>(_mockUnitOfWork.Object, _mockRepository.Object, _mockDataContractMapper.Object);

            var result = manager.DeleteAsync(1).Result;

            Assert.IsFalse(result);
            _mockRepository.Verify(m => m.GetByIdAsync(1), Times.Once);
            _mockRepository.Verify(m => m.Delete(It.IsAny<TestEntity>()), Times.Never);
            _mockUnitOfWork.Verify(m => m.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public void Test_Delete_EntityFound_ChangesSaved_ReturnTrue(){
            var testEntity = new TestEntity();
            
            _mockRepository.Setup(m => m.GetByIdAsync(1)).ReturnsAsync(testEntity);
            
            var manager = new CrudService<TestContext, TestEntity, TestDto>(_mockUnitOfWork.Object, _mockRepository.Object, _mockDataContractMapper.Object);

            var result = manager.DeleteAsync(1).Result;

            Assert.IsTrue(result);
            _mockRepository.Verify(m => m.GetByIdAsync(1), Times.Once);
            _mockRepository.Verify(m => m.Delete(testEntity), Times.Once);
            _mockUnitOfWork.Verify(m => m.SaveChangesAsync(), Times.Once);
        }
    }
}