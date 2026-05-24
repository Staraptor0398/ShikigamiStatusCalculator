#pragma once
#include <gtest/gtest.h>

#define TEST_ASSERT_EQUAL(expected, actual) \
EXPECT_EQ((expected), (actual))

#define TEST_ASSERT_NOT_EQUAL(expected, actual) \
EXPECT_NE((expected), (actual))

#define TEST_ASSERT_TRUE(actual) \
EXPECT_TRUE((actual))

#define TEST_ASSERT_FALSE(actual) \
EXPECT_FALSE((actual))

#define TEST_ASSERT_DOUBLE_EQUAL(expected, actual) \
EXPECT_DOUBLE_EQ((expected), (actual))

#define TEST_ASSERT_DOUBLE_NEAR(expected, actual, tolerance) \
EXPECT_NEAR((expected), (actual), (tolerance))
