-- Complete Database Seeding Script
-- Seeds banks, ratings, and historical data

SET NOCOUNT ON;

PRINT 'Starting complete database seeding...';
PRINT '';

-- Add missing banks if they don't exist
PRINT 'Checking and adding missing banks...';

IF NOT EXISTS (SELECT 1 FROM Banks WHERE BankCode = 'bank-gamma')
BEGIN
    INSERT INTO Banks (BankCode, ViewCount, CreatedDate)
    VALUES ('bank-gamma', 220, GETUTCDATE());
    PRINT 'Added bank-gamma';
END

IF NOT EXISTS (SELECT 1 FROM Banks WHERE BankCode = 'bank-epsilon')
BEGIN
    INSERT INTO Banks (BankCode, ViewCount, CreatedDate)
    VALUES ('bank-epsilon', 180, GETUTCDATE());
    PRINT 'Added bank-epsilon';
END

PRINT 'Banks setup complete';
PRINT '';

-- Add bank ratings if they don't exist
PRINT 'Checking and adding bank ratings...';

DECLARE @AlphaId INT = (SELECT BankId FROM Banks WHERE BankCode = 'bank-alpha');
DECLARE @BetaId INT = (SELECT BankId FROM Banks WHERE BankCode = 'bank-beta');
DECLARE @GammaId INT = (SELECT BankId FROM Banks WHERE BankCode = 'bank-gamma');
DECLARE @DeltaId INT = (SELECT BankId FROM Banks WHERE BankCode = 'bank-delta');
DECLARE @EpsilonId INT = (SELECT BankId FROM Banks WHERE BankCode = 'bank-epsilon');

-- Alpha Bank - High quality
IF NOT EXISTS (SELECT 1 FROM BankRatings WHERE BankId = @AlphaId)
BEGIN
    INSERT INTO BankRatings (BankId, CriteriaId, RatingValue, RatingDate) VALUES
    (@AlphaId, 1, 9.2, GETUTCDATE()),
    (@AlphaId, 2, 8.5, GETUTCDATE()),
    (@AlphaId, 3, 9.0, GETUTCDATE()),
    (@AlphaId, 4, 8.8, GETUTCDATE()),
    (@AlphaId, 5, 9.1, GETUTCDATE());
    PRINT 'Added ratings for bank-alpha';
END

-- Beta Bank - Moderate
IF NOT EXISTS (SELECT 1 FROM BankRatings WHERE BankId = @BetaId)
BEGIN
    INSERT INTO BankRatings (BankId, CriteriaId, RatingValue, RatingDate) VALUES
    (@BetaId, 1, 7.5, GETUTCDATE()),
    (@BetaId, 2, 7.0, GETUTCDATE()),
    (@BetaId, 3, 6.8, GETUTCDATE()),
    (@BetaId, 4, 6.5, GETUTCDATE()),
    (@BetaId, 5, 7.8, GETUTCDATE());
    PRINT 'Added ratings for bank-beta';
END

-- Gamma Bank - Excellent digital
IF NOT EXISTS (SELECT 1 FROM BankRatings WHERE BankId = @GammaId)
BEGIN
    INSERT INTO BankRatings (BankId, CriteriaId, RatingValue, RatingDate) VALUES
    (@GammaId, 1, 8.9, GETUTCDATE()),
    (@GammaId, 2, 9.5, GETUTCDATE()),
    (@GammaId, 3, 9.3, GETUTCDATE()),
    (@GammaId, 4, 9.8, GETUTCDATE()),
    (@GammaId, 5, 9.0, GETUTCDATE());
    PRINT 'Added ratings for bank-gamma';
END

-- Delta Bank - Lower ratings
IF NOT EXISTS (SELECT 1 FROM BankRatings WHERE BankId = @DeltaId)
BEGIN
    INSERT INTO BankRatings (BankId, CriteriaId, RatingValue, RatingDate) VALUES
    (@DeltaId, 1, 6.2, GETUTCDATE()),
    (@DeltaId, 2, 5.8, GETUTCDATE()),
    (@DeltaId, 3, 6.5, GETUTCDATE()),
    (@DeltaId, 4, 5.5, GETUTCDATE()),
    (@DeltaId, 5, 6.8, GETUTCDATE());
    PRINT 'Added ratings for bank-delta';
END

-- Epsilon Bank - Premium
IF NOT EXISTS (SELECT 1 FROM BankRatings WHERE BankId = @EpsilonId)
BEGIN
    INSERT INTO BankRatings (BankId, CriteriaId, RatingValue, RatingDate) VALUES
    (@EpsilonId, 1, 9.5, GETUTCDATE()),
    (@EpsilonId, 2, 7.8, GETUTCDATE()),
    (@EpsilonId, 3, 9.2, GETUTCDATE()),
    (@EpsilonId, 4, 8.9, GETUTCDATE()),
    (@EpsilonId, 5, 9.4, GETUTCDATE());
    PRINT 'Added ratings for bank-epsilon';
END

PRINT 'Bank ratings setup complete';
PRINT '';

-- Display current state
PRINT 'Current database state:';
SELECT B.BankCode, COUNT(BR.RatingId) AS RatingCount
FROM Banks B
LEFT JOIN BankRatings BR ON B.BankId = BR.BankId
GROUP BY B.BankCode
ORDER BY B.BankCode;
PRINT '';

PRINT 'Initial seeding complete!';
