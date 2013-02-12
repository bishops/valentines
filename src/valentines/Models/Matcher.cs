using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace valentines.Models
{
    public class Matcher
    {
        private ValentinesDataContext db;
        public Matcher(ValentinesDataContext DB)
        {
            this.db = DB;
        }

        public void computeMatches()
        {
            // Launch
            var ulist = db.aspnet_Users.OrderBy(u => u.UserId).ToList();
            for (int i = 0; i < ulist.Count; i++)
            {
                for (int j = i + 1; j < ulist.Count; j++)
                {
                    MakeMatch(ulist[i], ulist[j]);
                }
            }
        }

        public void MakeMatch(aspnet_User one, aspnet_User two)
        {
            // Compute
            var totalNumQuestions = db.Questions.Count();
            var scoreSame = 0;
            foreach (var q in db.Questions)
            {
                var ansone = db.Responses.Where(u => u.UserId == one.UserId && u.QuestionId == q.Id).SingleOrDefault().AnswerId;
                var anstwo = db.Responses.Where(u => u.UserId == two.UserId && u.QuestionId == q.Id).SingleOrDefault().AnswerId;
                if (ansone == anstwo)
                {
                    scoreSame++;
                }
            }
            double ratio = scoreSame / totalNumQuestions;

            // Add noise
            var noiseInt = new Random().Next(1, 20);
            if (new Random().Next(0, 1) == 1)
            {
                noiseInt *= -1;
            }
            double noise = (double)noiseInt * 100;
            if (ratio + noise > 1 || ratio + noise < 0)
            {
                noise *= -1;
            }
            ratio += noise;

            // Get profiles
            var profile1 = AccountProfile.GetProfileOfUser(one.UserName);
            var profile2 = AccountProfile.GetProfileOfUser(two.UserName);

            // Write to DB
            var time = DateTime.Now;

            var m_one = new Match();
            m_one.RequestUser = one.UserId;
            m_one.MatchedUser = two.UserId;
            m_one.MatchedSex = (profile2.Sex == 2);
            m_one.AreSameGrade = profile1.Grade == profile2.Grade;
            m_one.CompatibilityIndex = ratio;
            m_one.DateCalculated = time;

            db.Matches.InsertOnSubmit(m_one);

            var m_two = new Match();
            m_two.RequestUser = two.UserId;
            m_two.MatchedUser = one.UserId;
            m_two.MatchedSex = (profile1.Sex == 2);
            m_two.AreSameGrade = profile1.Grade == profile2.Grade;
            m_two.CompatibilityIndex = ratio;
            m_two.DateCalculated = time;

            db.Matches.InsertOnSubmit(m_two);

            db.SubmitChanges();
        }
    }

    public partial class Match
    {
        public string FullName
        {
            get;
            internal set;
        }
        public int Grade
        {
            get;
            internal set;
        }
        public int PositionOnTheirListAllSchool
        {
            get;
            internal set;
        }
        public int PositionOnTheirListYourGrade
        {
            get;
            internal set;
        }

        public void FillProperties()
        {
            var profile = AccountProfile.GetProfileOfUser(this.aspnet_User1.UserName);
            FullName = profile.FullName;
            Grade = profile.Grade;

            // What sex is the current user?
            bool selectedSex = false; // male
            if(AccountProfile.CurrentUser.Sex == 2)
            {
                selectedSex = true; // female
            }

            // All school for whatever gender you are
            var allSchoolYourGender = this.aspnet_User1.Matches.Where(m => m.MatchedSex == selectedSex).OrderByDescending(m => m.CompatibilityIndex);
            // Figure out your position
            var result = allSchoolYourGender
                .Select((x, i) => new { Item = x, Index = i })
                .Where(itemWithIndex => itemWithIndex.Item.MatchedUser == Current.UserID.Value)
                .FirstOrDefault();

            int index = -1;
            if (result != null)
                index = result.Index;
            PositionOnTheirListAllSchool = index;

            if (!this.AreSameGrade)
            {
                PositionOnTheirListYourGrade = -1;
            }
            else
            {
                // Your grade for whatever gender you are
                var yourGradeYourGender = this.aspnet_User1.Matches.Where(m => m.MatchedSex == selectedSex && m.AreSameGrade==true).OrderByDescending(m => m.CompatibilityIndex);
                // Figure out your position
                var resultG = allSchoolYourGender
                    .Select((x, i) => new { Item = x, Index = i })
                    .Where(itemWithIndex => itemWithIndex.Item.MatchedUser == Current.UserID.Value)
                    .FirstOrDefault();

                int indexG = -1;
                if (resultG != null)
                    indexG = resultG.Index;
                PositionOnTheirListYourGrade = indexG;
            }
        }
    }
}